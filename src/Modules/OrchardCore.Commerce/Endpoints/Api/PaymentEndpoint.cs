#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints.Api;
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
         string? shoppingCartId,
         IAuthorizationService authorizationService,
         HttpContext httpContext,
         IHtmlLocalizer<IPaymentService> htmlLocalizer,
         IPaymentService paymentService
       )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (await paymentService.CreatePendingOrderFromShoppingCartAsync(shoppingCartId, mustBeFree: true) is { } order)
        {
            var result = await paymentService.UpdateAndRedirectToFinishedOrderAsync(order, shoppingCartId, htmlLocalizer);
            return TypedResults.Ok(result);
        }

        return TypedResults.NotFound();
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
         string paymentProviderName,
         string? orderId,
         [FromQuery] string? shoppingCartId,
         IAuthorizationService authorizationService,
         HttpContext httpContext,
         IPaymentService paymentService
       )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (await paymentService.CallBackAsync(paymentProviderName, orderId, shoppingCartId) is { } result)
        {
            return TypedResults.Ok(result);
        }

        return TypedResults.NotFound();
    }
}
