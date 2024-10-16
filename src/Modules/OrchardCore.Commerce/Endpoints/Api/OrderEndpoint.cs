using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.ContentManagement;
using System.Security.Claims;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Abstractions.Constants.ContentTypes;

namespace OrchardCore.Commerce.Endpoints.Api;

public static class OrderEndpoint
{
    private const string ApiPath = "api/order";

    public static IEndpointRouteBuilder AddNewOrderEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings(ApiPath + "/test", NewOrderAsync);

        return builder;
    }

    // Create order content item
    private static async Task<IResult> NewOrderAsync(
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IContentManager contentManager,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceOrderApi))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var order = await contentManager.NewAsync(Order);

        return TypedResults.Ok(order);
    }
}
