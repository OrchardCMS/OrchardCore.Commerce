#nullable enable
using Lombiq.HelpfulLibraries.AspNetCore.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.Endpoints.Extensions;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Commerce.Endpoints.ViewModels;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints.Api;
public static class ShoppingCartLineEndpoint
{
    public static IEndpointRouteBuilder AddEstimateProductAsyncEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/ShoppingCart/EstimateProduct/{shoppingCartId?}", EstimateProductAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> EstimateProductAsync(
        string? shoppingCartId,
        [FromBody] EstimateProductViewModel estimateProductVM,
        IAuthorizationService authorizationService,
        HttpContext httpContext,
        IShoppingCartHelpers shoppingCartHelpers
        )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        ShoppingCartLineViewModel shoppingCartLineViewModel;

        try
        {
            shoppingCartLineViewModel = await shoppingCartHelpers
                .EstimateProductAsync(shoppingCartId, estimateProductVM.Sku, estimateProductVM.Shipping, estimateProductVM.Billing);
        }
        catch (FrontendException ex)
        {
            var errors = ex.HtmlMessages;
            return TypedResults.Problem(errors.ConvertLocalizedHtmlStringList());
        }

        if (shoppingCartLineViewModel == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(shoppingCartLineViewModel);
    }

    public static IEndpointRouteBuilder AddCreateShoppingCartViewModelEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/ShoppingCart/CreateShoppingCartViewModel", CreateShoppingCartViewModelAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> CreateShoppingCartViewModelAsync(
         [FromBody] CreateShoppingCartViewModel createShoppingCartVM,
         IAuthorizationService authorizationService,
         HttpContext httpContext,
         IShoppingCartHelpers shoppingCartHelpers
       )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        ShoppingCartViewModel shoppingCartViewModel;
        try
        {
            shoppingCartViewModel = await shoppingCartHelpers.CreateShoppingCartViewModelAsync(
                createShoppingCartVM.ShoppingCartId, createShoppingCartVM.Shipping, createShoppingCartVM.Billing);
        }
        catch (FrontendException ex)
        {
            shoppingCartViewModel = new ShoppingCartViewModel();
            shoppingCartViewModel.InvalidReasons.AddRange(ex.HtmlMessages);
        }

        if (shoppingCartViewModel == null)
            return TypedResults.NotFound();

        return TypedResults.Created("api/ShoppingCart/CreateShoppingCartViewModel", shoppingCartViewModel);
    }

    public static IEndpointRouteBuilder AddAddItemEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/ShoppingCart/AddItem", AddItemAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> AddItemAsync(
         [FromBody] AddItemViewModel addItemVM,
         IAuthorizationService authorizationService,
         HttpContext httpContext,
         IShoppingCartService shoppingCartService,
         IHtmlLocalizer<AddItemViewModel> htmlLocalizer
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

    public static IEndpointRouteBuilder AddUpdateEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("api/ShoppingCart/Update", UpdateAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> UpdateAsync(
         [FromBody] UpdateViewModel updateVM,
         IAuthorizationService authorizationService,
         HttpContext httpContext,
         IShoppingCartService shoppingCartService,
         IHtmlLocalizer<UpdateViewModel> htmlLocalizer
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
        builder.MapDelete("api/ShoppingCart/Delete", RemoveLineAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> RemoveLineAsync(
         [FromBody] RemoveLineViewModel removeLineVM,
         IAuthorizationService authorizationService,
         HttpContext httpContext,
         IShoppingCartService shoppingCartService,
         IHtmlLocalizer<RemoveLineViewModel> htmlLocalizer
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
        builder.MapGet("api/ShoppingCart/RetrieveCart/{shoppingCartId?}", RetrieveAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> RetrieveAsync(
        string? shoppingCartId,
        IAuthorizationService authorizationService,
        HttpContext httpContext,
        IShoppingCartPersistence shoppingCartPersistence
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
