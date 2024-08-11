#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.EndPoints.Api;
public static class StripeEndpoint
{
    public static IEndpointRouteBuilder AddStripeMiddlewareEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/checkout/middleware/Stripe", AddStripeMiddlewareAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    private static async Task<IResult> AddStripeMiddlewareAsync(
         IHtmlLocalizer<IStripePaymentService> htmlLocalizer,
         IStripePaymentService stripePaymentService,
         [FromQuery(Name = "payment_intent")] string? paymentIntent = null,
         [FromQuery] string? shoppingCartId = null
       )
    {
        var result = await stripePaymentService.PaymentConfirmationAsync(paymentIntent, shoppingCartId, htmlLocalizer);
        return TypedResults.Ok(result);
    }
}
