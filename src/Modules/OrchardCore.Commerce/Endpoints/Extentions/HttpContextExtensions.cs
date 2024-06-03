using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Modules;

public static class HttpContextExtensions
{
    /// <summary>
    /// Makes <see cref="HttpContext.RequestServices"/> aware of the current <see cref="ShellScope"/>.
    /// </summary>
    public static HttpContext UseShellScopeServices(this HttpContext httpContext)
    {
        httpContext.RequestServices = new ShellScopeServices(httpContext.RequestServices);
        return httpContext;
    }

    public static IResult ChallengeOrForbid(this HttpContext httpContext, params string[] authenticationSchemes) =>
        httpContext.User?.Identity?.IsAuthenticated == true
            ? TypedResults.Forbid(authenticationSchemes: authenticationSchemes)
            : TypedResults.Challenge(authenticationSchemes: authenticationSchemes);
}
