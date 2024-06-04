using Microsoft.AspNetCore.Http;

namespace OrchardCore.Modules;

/// <summary>
/// Copy from orchardcore 2.0 branch, will be deleted after merge to OC 2.0.
/// </summary>
public static class HttpContextExtensions
{
    public static IResult ChallengeOrForbid(this HttpContext httpContext, params string[] authenticationSchemes) =>
        httpContext.User?.Identity?.IsAuthenticated == true
            ? TypedResults.Forbid(authenticationSchemes: authenticationSchemes)
            : TypedResults.Challenge(authenticationSchemes: authenticationSchemes);
}
