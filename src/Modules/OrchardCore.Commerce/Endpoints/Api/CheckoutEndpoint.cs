using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints.Api;
public static class CheckoutEndpoint
{
    public static IEndpointRouteBuilder AddCheckoutAsyncEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/Checkout/{shoppingCartId?}", CheckoutAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> CheckoutAsync(
        string shoppingCartId,
        IAuthorizationService authorizationService,
        HttpContext httpContext,
        IPaymentService paymentService
        )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (await paymentService.CreateCheckoutViewModelAsync(shoppingCartId) is not { } checkoutViewModel ||
            checkoutViewModel.IsInvalid)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(checkoutViewModel);
    }
}
