using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions.Constants;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Endpoints.Permissions;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Endpoints.Api;
public static class PaymentEndpoint
{
    public static IEndpointRouteBuilder AddFreeEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/checkout/free/{shoppingCartId?}", AddFreeAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> AddFreeAsync(
         [FromRoute] string? shoppingCartId,
         [FromServices] IAuthorizationService authorizationService,
         HttpContext httpContext,
         [FromServices] IPaymentService paymentService
       )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiPayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var result = await paymentService.CheckoutWithoutPaymentAsync(shoppingCartId);
        return TypedResults.Ok(result);
    }

    public static IEndpointRouteBuilder AddCallbackEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/checkout/callback/{paymentProviderName}/{orderId?}", AddCallbackAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> AddCallbackAsync(
         [FromRoute] string paymentProviderName,
         [FromRoute] string? orderId,
         [FromQuery] string? shoppingCartId,
         [FromServices] IAuthorizationService authorizationService,
         HttpContext httpContext,
         [FromServices] IPaymentService paymentService
       )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiPayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        if (paymentProviderName.EqualsOrdinalIgnoreCase("Stripe"))
        {
            return TypedResults.BadRequest("Stripe payment uses ~/checkout/middleware/Stripe, not ~/checkout/callback/Stripe.");
        }

        if (string.IsNullOrWhiteSpace(orderId))
        {
            orderId = null;
        }

        if (await paymentService.CallBackAsync(paymentProviderName, orderId, shoppingCartId) is { } result)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.NotFound();
    }

    // Might not be needed.
    public static IEndpointRouteBuilder AddPaymentRequestEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/checkout/checkout/paymentrequest/{orderId}", PaymentRequestAsync)
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> PaymentRequestAsync(
        [FromRoute] string orderId,
        [FromServices] IContentManager contentManager,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IEnumerable<IPaymentProvider> paymentProviders,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiPayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        if (await contentManager.GetAsync(orderId) is not { } order ||
            order.As<OrderPart>() is not { } orderPart)
        {
            return TypedResults.BadRequest();
        }

        // If there are no line items, there is nothing to be done.
        if (!orderPart.LineItems.Any())
        {
            return TypedResults.Ok("This Order contains no line items, so there is nothing to be paid.");
        }

        // If status is not Pending, there is nothing to be done.
        if (!string.Equals(orderPart.Status.Text, OrderStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.Ok("This Order is no longer pending.");
        }

        var singleCurrencyTotal = orderPart.LineItems.Select(item => item.LinePrice).Sum();
        if (singleCurrencyTotal.Value <= 0)
        {
            return TypedResults.Ok("This Order's line items have no cost, so there is nothing to be paid.");
        }

        var viewModel = new PaymentViewModel(orderPart, singleCurrencyTotal, singleCurrencyTotal);
        await viewModel.WithProviderDataAsync(paymentProviders, isPaymentRequest: true);

        return TypedResults.Ok(viewModel);
    }
}
