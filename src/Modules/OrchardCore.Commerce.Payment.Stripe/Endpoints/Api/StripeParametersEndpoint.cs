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
using OrchardCore.Commerce.Payment.Stripe.Helpers;
using OrchardCore.ContentManagement;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Payment.Stripe.Endpoints.Constants.Endpoints;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;
public static class StripeParametersEndpoint
{
    // Create stripe confirm parameters endpoint
    public static IEndpointRouteBuilder AddStripeConfirmParametersEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings($"{StripePaymentApiPath}/confirm-parameters", GetStripeConfirmParametersAsync);
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

        // Return the model as a JSON result. We use the JsonSerializerOptions to configure the JSON serialization for
        // the specific configuration Stripe requires as ConfirmParameters.
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
        builder.MapGetWithDefaultSettings($"{StripePaymentApiPath}/total", GetStripeTotalAsync);
        return builder;
    }

    private static async Task<IResult> GetStripeTotalAsync(
        [FromQuery] string? shoppingCartId,
        [FromServices] IShoppingCartService shoppingCartService,
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
            Amount = AmountHelpers.GetPaymentAmount(total),
            total.Currency,
        });
    }

    public static IEndpointRouteBuilder AddStripePublicKeyEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGetWithDefaultSettings($"{StripePaymentApiPath}/public-key", GetStripePublicKeyAsync);
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
}
