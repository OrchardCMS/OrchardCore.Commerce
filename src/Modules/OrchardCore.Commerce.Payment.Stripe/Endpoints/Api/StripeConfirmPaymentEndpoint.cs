#nullable enable
using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using System;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Payment.Stripe.Endpoints.Constants.Endpoints;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;
public static class StripeConfirmPaymentEndpoint
{
    [Obsolete("This endpoint is obsolete and will be removed in a future version. Use checkout/stripe/middleware instead.")]
    public static IEndpointRouteBuilder AddStripeMiddlewareEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings($"{StripePaymentApiPath}/middleware", AddStripeMiddlewareAsync);
        return builder;
    }

    [Obsolete("This endpoint is obsolete and will be removed in a future version. Use checkout/stripe/middleware instead.")]
    private static async Task<IResult> AddStripeMiddlewareAsync(
         [FromQuery] string? shoppingCartId,
         [FromServices] IStripePaymentService stripePaymentService,
         [FromServices] IAuthorizationService authorizationService,
         HttpContext httpContext,
         [FromQuery(Name = "payment_intent")] string? paymentIntentId = null)
    {
        var result = await StripePaymentOrderConfirmationAsync(
            shoppingCartId,
            stripePaymentService,
            authorizationService,
            httpContext,
            paymentIntentId);
        return TypedResults.Ok(result);
    }

    public static IEndpointRouteBuilder AddStripePaymentOrderConfirmationEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings($"{StripePaymentApiPath}", StripePaymentOrderConfirmationAsync);
        return builder;
    }

    private static async Task<IResult> StripePaymentOrderConfirmationAsync(
        [FromQuery] string? shoppingCartId,
        [FromServices] IStripePaymentService stripePaymentService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext,
        [FromQuery(Name = "payment_intent")] string? paymentIntentId = null)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var result = await stripePaymentService.PaymentConfirmationAsync(
            paymentIntentId,
            shoppingCartId,
            paymentIntentId == null);

        return TypedResults.Ok(result);
    }
}
