using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Commerce.Models;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Endpoints.Api;
public static class UserAddressEndpoint
{
    public static IEndpointRouteBuilder AddUserAddressEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/user/UserAddresses", HandleAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> HandleAsync(
        IAuthorizationService authorizationService,
        HttpContext httpContext
        )
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var userAddresses = (await httpContext.GetUserAddressAsync()) ?? new UserAddressesPart();

        return TypedResults.Ok(userAddresses);
    }
}
