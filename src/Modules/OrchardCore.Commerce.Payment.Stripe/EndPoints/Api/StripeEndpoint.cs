#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Modules;
using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.EndPoints.Api;
public static class StripeEndpoint
{
    public static IEndpointRouteBuilder AddStripePublicKeyEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/checkout/stripe/public-key", GetStripePublicKeyAsync)
            .RequireAuthorization("Api")
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
            .RequireAuthorization("Api")
            .DisableAntiforgery();

        return builder;
    }

    [HttpPost]
    public static async Task<IResult> CreatePaymentIntentAsync(
        [FromBody] string amount,
        [FromServices] PaymentIntentService paymentIntentService
        )
    {
        var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = long.Parse(amount),
            Currency = "eur",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true, },
        });

        return TypedResults.Ok(new
        {
            clientSecret = paymentIntent.ClientSecret,
            // [DEV]: For demo purposes only, you should avoid exposing the PaymentIntent ID in the client-side code.
            dpmCheckerLink = $"https://dashboard.stripe.com/settings/payment_methods/review?transaction_id={paymentIntent.Id}",
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
