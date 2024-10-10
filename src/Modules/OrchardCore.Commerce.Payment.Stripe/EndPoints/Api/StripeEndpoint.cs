#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.EndPoints.Models;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Modules;
using OrchardCore.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.EndPoints.Api;
public static class StripeEndpoint
{
    // Get total amount
    public static IEndpointRouteBuilder AddStripeTotalEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/checkout/stripe/total/{shoppingCartId?}", GetStripeTotalAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    private static async Task<IResult> GetStripeTotalAsync(
        [FromRoute] string? shoppingCartId,
        [FromServices] IShoppingCartService shoppingCartService,
        [FromServices] IStripePaymentService stripePaymentService
        )
    {
        var shoppingCartViewModel = await shoppingCartService.GetAsync(shoppingCartId);
        if (shoppingCartViewModel == null)
        {
            return TypedResults.Ok();
        }

        var total = shoppingCartViewModel.Totals.Single();
        return TypedResults.Ok(new
        {
            Amount = stripePaymentService.GetPaymentAmount(total),
            total.Currency,
        });
    }

    public static IEndpointRouteBuilder AddStripePublicKeyEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/checkout/stripe/public-key", GetStripePublicKeyAsync)
            .DisableAntiforgery();

        return builder;
    }

    private static async Task<IResult> GetStripePublicKeyAsync(
        [FromServices] IStripePaymentService stripePaymentService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext
        )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var publicKey = await stripePaymentService.GetPublicKeyAsync();
        return TypedResults.Ok(publicKey);
    }

    // CreatePaymentIntent
    public static IEndpointRouteBuilder AddStripePaymentIntentEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/checkout/stripe/payment-intent", CreatePaymentIntentAsync)
            .DisableAntiforgery();

        return builder;
    }

    [HttpPost]
    public static async Task<IResult> CreatePaymentIntentAsync(
        [FromBody] CreatePaymentIntentViewModel viewModel,
        [FromServices] ISiteService siteService,
        [FromServices] IStripePaymentService stripePaymentService,
        [FromServices] IShoppingCartService shoppingCartService,
        [FromServices] IEnumerable<IPaymentProvider> paymentProviders
        )
    {
        var shoppingCartViewModel = await shoppingCartService.GetAsync(viewModel.ShoppingCartId);
        var total = shoppingCartViewModel.Totals.Single();
        var paymentIntent = await stripePaymentService.CreatePaymentIntentAsync(total);

        return TypedResults.Ok(new
        {
            clientSecret = paymentIntent.ClientSecret,
        });
    }

    public static IEndpointRouteBuilder AddStripeMiddlewareEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/checkout/middleware/Stripe/{shoppingCartId?}", AddStripeMiddlewareAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    private static async Task<IResult> AddStripeMiddlewareAsync(
         [FromRoute] string? shoppingCartId,
         [FromServices] IStripePaymentService stripePaymentService,
         [FromQuery(Name = "payment_intent")] string? paymentIntent = null
       )
    {
        var result = await stripePaymentService.PaymentConfirmationAsync(paymentIntent, shoppingCartId);
        return TypedResults.Ok(result);
    }
}
