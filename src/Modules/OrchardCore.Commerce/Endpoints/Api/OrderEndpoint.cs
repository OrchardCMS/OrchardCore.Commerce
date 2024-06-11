using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Commerce.Endpoints.ViewModels;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints.Api;
public static class OrderEndpoint
{
    public static IEndpointRouteBuilder AddOrderLineItemEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/OrderLineItems", CreateOrderLineItemViewModelsAndTotalAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> CreateOrderLineItemViewModelsAndTotalAsync(
        [FromBody] CreateOrderLineItemViewModel createOrderLineItemVM,
        IOrderLineItemService orderLineItemService,
        IAuthorizationService authorizationService,
        HttpContext httpContext
        )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var model = await orderLineItemService
            .CreateOrderLineItemViewModelsAndTotalAsync(createOrderLineItemVM.LineItems, createOrderLineItemVM.OrderPart);
        return TypedResults.Ok(model);
    }
}
