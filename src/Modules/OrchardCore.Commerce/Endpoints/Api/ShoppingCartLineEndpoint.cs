#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Commerce.Endpoints.ViewModels;
using OrchardCore.Modules;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints.Api;
public static class ShoppingCartLineEndpoint
{
    public static IEndpointRouteBuilder AddAddItemEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/shoppingcart/add-item", AddItemAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> AddItemAsync(
         [FromBody] AddItemViewModel addItemVM,
         [FromServices] IAuthorizationService authorizationService,
         [FromServices] HttpContext httpContext,
         [FromServices] IShoppingCartService shoppingCartService,
         [FromServices] IHtmlLocalizer<AddItemViewModel> htmlLocalizer
       )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (string.IsNullOrEmpty(addItemVM.ShoppingCartId)) { addItemVM.ShoppingCartId = Guid.NewGuid().ToString("n"); }

        var errored = await shoppingCartService.AddItemAsync(addItemVM.Line, addItemVM.Token, addItemVM.ShoppingCartId);
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
        builder.MapPut("api/shoppingcart/update", UpdateAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> UpdateAsync(
         [FromBody] UpdateViewModel updateVM,
         [FromServices] IAuthorizationService authorizationService,
         [FromServices] HttpContext httpContext,
         [FromServices] IShoppingCartService shoppingCartService,
         [FromServices] IHtmlLocalizer<UpdateViewModel> htmlLocalizer
       )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var errored = await shoppingCartService.UpdateAsync(updateVM.Cart, updateVM.Token, updateVM.ShoppingCartId);
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
        builder.MapDelete("api/shoppingcart/delete", RemoveLineAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> RemoveLineAsync(
         [FromBody] RemoveLineViewModel removeLineVM,
         [FromServices] IAuthorizationService authorizationService,
         [FromServices] HttpContext httpContext,
         [FromServices] IShoppingCartService shoppingCartService,
         [FromServices] IHtmlLocalizer<RemoveLineViewModel> htmlLocalizer
       )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var errored = await shoppingCartService.RemoveLineAsync(removeLineVM.Line, removeLineVM.ShoppingCartId);
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

    public static IEndpointRouteBuilder AddRetrieveAsyncEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/shoppingcart/retrieve-cart/{shoppingCartId?}", RetrieveAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> RetrieveAsync(
        [FromRoute] string? shoppingCartId,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] HttpContext httpContext,
        [FromServices] IShoppingCartPersistence shoppingCartPersistence
        )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var cart = await shoppingCartPersistence.RetrieveAsync(shoppingCartId);

        if (cart == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(cart);
    }
}
