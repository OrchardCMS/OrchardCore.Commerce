#nullable enable
using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Commerce.Endpoints.ViewModels;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints.Api;

public static class ShoppingCartLineEndpoint
{
    private const string ApiPath = "api/shoppingcart/{shoppingCartId?}";

    public static IEndpointRouteBuilder AddGetCartEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGetWithDefaultSettings(ApiPath, GetCartAsync);

        return builder;
    }

    private static async Task<IResult> GetCartAsync(
        [FromRoute] string? shoppingCartId,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IShoppingCartService shoppingCartService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceShoppingCartApi))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var cart = await shoppingCartService.GetAsync(shoppingCartId);

        if (cart == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(cart);
    }

    public static IEndpointRouteBuilder AddAddItemEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings(ApiPath, AddItemAsync);

        return builder;
    }

    private static async Task<IResult> AddItemAsync(
        [FromRoute] string? shoppingCartId,
        [FromBody] AddItemViewModel viewModel,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IShoppingCartService shoppingCartService,
        [FromServices] IHtmlLocalizer<AddItemViewModel> htmlLocalizer,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceShoppingCartApi))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var errored = await shoppingCartService.AddItemAsync(viewModel.Line, viewModel.Token, shoppingCartId);
        if (string.IsNullOrEmpty(errored))
        {
            return TypedResults.Created();
        }

        var problemDetails = new ProblemDetails
        {
            Detail = errored,
            Status = 500,
            Title = htmlLocalizer["Error"].Value,
        };
        return TypedResults.Problem(problemDetails);
    }

    public static IEndpointRouteBuilder AddUpdateEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPutWithDefaultSettings(ApiPath, UpdateAsync);

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> UpdateAsync(
        [FromRoute] string? shoppingCartId,
        [FromBody] UpdateViewModel viewModel,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IShoppingCartService shoppingCartService,
        [FromServices] IHtmlLocalizer<UpdateViewModel> htmlLocalizer,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceShoppingCartApi))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var errored = await shoppingCartService.UpdateAsync(viewModel.Cart, viewModel.Token, shoppingCartId);
        if (string.IsNullOrEmpty(errored))
        {
            return TypedResults.NoContent();
        }

        var problemDetails = new ProblemDetails
        {
            Detail = errored,
            Status = 500,
            Title = htmlLocalizer["Error"].Value,
        };
        return TypedResults.Problem(problemDetails);
    }

    public static IEndpointRouteBuilder AddRemoveLineEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapDeleteWithDefaultSettings(ApiPath, RemoveLineAsync);

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> RemoveLineAsync(
        [FromRoute] string? shoppingCartId,
        [FromBody] RemoveLineViewModel viewModel,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IShoppingCartService shoppingCartService,
        [FromServices] IHtmlLocalizer<RemoveLineViewModel> htmlLocalizer,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var errored = await shoppingCartService.RemoveLineAsync(viewModel.Line, shoppingCartId);
        if (string.IsNullOrEmpty(errored))
        {
            return TypedResults.NoContent();
        }

        var problemDetails = new ProblemDetails
        {
            Detail = errored,
            Status = 500,
            Title = htmlLocalizer["Error"].Value,
        };

        return TypedResults.Problem(problemDetails);
    }
}
