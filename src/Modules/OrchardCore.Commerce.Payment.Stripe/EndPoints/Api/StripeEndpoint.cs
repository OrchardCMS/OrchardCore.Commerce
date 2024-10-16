#nullable enable
using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Endpoints;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;
public static class StripeEndpoint
{
    // Create stripe confirmparameters endpoint
    public static IEndpointRouteBuilder AddStripeConfirmParametersEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings("api/checkout/stripe/confirm-parameters", GetStripeConfirmParametersAsync);
        return builder;
    }

    // The GetConfirmPaymentParametersAsync method is used to get the confirm payment parameters.
    private static async Task<IResult> GetStripeConfirmParametersAsync(
        [FromBody] ConfirmParametersViewModel confirmParametersViewModel,
        [FromServices] IStripePaymentService stripePaymentService,
        [FromServices] IContentManager contentManager,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        // Check if the user is authorized to get the confirm payment parameters.
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var order = await contentManager.GetAsync(confirmParametersViewModel.OrderId);

        var model = await stripePaymentService.GetStripeConfirmParametersAsync(
            confirmParametersViewModel.ReturnUrl,
            order);

        return TypedResults.Json(
            model,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

            });
    }

    public static IEndpointRouteBuilder AddStripeTotalEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGetWithDefaultSettings("api/checkout/stripe/total", GetStripeTotalAsync);
        return builder;
    }

    private static async Task<IResult> GetStripeTotalAsync(
        [FromQuery] string? shoppingCartId,
        [FromServices] IShoppingCartService shoppingCartService,
        [FromServices] IStripePaymentService stripePaymentService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

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
        builder.MapGetWithDefaultSettings("api/checkout/stripe/public-key", GetStripePublicKeyAsync);
        return builder;
    }

    private static async Task<IResult> GetStripePublicKeyAsync(
        [FromServices] IStripePaymentService stripePaymentService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var publicKey = await stripePaymentService.GetPublicKeyAsync();
        return TypedResults.Ok(publicKey);
    }

    public static IEndpointRouteBuilder AddStripePaymentIntentGetEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGetWithDefaultSettings("api/checkout/stripe/payment-intent", GetPaymentIntentAsync);
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
        builder.MapPostWithDefaultSettings("api/checkout/stripe/payment-intent", CreatePaymentIntentAsync);
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

    public static IEndpointRouteBuilder AddStripeMiddlewareEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings("api/checkout/middleware/stripe", AddStripeMiddlewareAsync);
        return builder;
    }

    private static async Task<IResult> AddStripeMiddlewareAsync(
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
