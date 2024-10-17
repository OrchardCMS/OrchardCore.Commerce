#nullable enable
using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Payment.Stripe.Endpoints.Constants.Endpoints;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;
public static class StripePaymentIntentEndpoint
{
    public static IEndpointRouteBuilder AddStripePaymentIntentGetEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGetWithDefaultSettings($"{StripePaymentApiPath}/payment-intent", GetPaymentIntentAsync);
        return builder;
    }

    private static async Task<IResult> GetPaymentIntentAsync(
        [FromQuery] string paymentIntentId,
        [FromServices] IStripePaymentService stripePaymentService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext
        )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var paymentIntent = await stripePaymentService.GetPaymentIntentAsync(paymentIntentId);
        return TypedResults.Ok(paymentIntent);
    }

    public static IEndpointRouteBuilder AddStripePaymentIntentPostEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings($"{StripePaymentApiPath}/payment-intent", CreatePaymentIntentAsync);
        return builder;
    }

    private static async Task<IResult> CreatePaymentIntentAsync(
        [FromBody] CreatePaymentIntentWithOrderViewModel viewModel,
        [FromServices] IStripePaymentService stripePaymentService,
        [FromServices] IShoppingCartService shoppingCartService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var shoppingCartViewModel = await shoppingCartService.GetAsync(viewModel.ShoppingCartId);
        var total = shoppingCartViewModel.Totals.Single();
        var paymentIntent = await stripePaymentService.CreatePaymentIntentAsync(total);

        var order = await stripePaymentService.CreateOrUpdateOrderFromShoppingCartAsync(
            updateModelAccessor: null,
            viewModel.ShoppingCartId,
            paymentIntent.Id,
            viewModel.OrderPart);

        return TypedResults.Ok(new { clientSecret = paymentIntent.ClientSecret, orderContentItemId = order.ContentItemId });
    }
}
