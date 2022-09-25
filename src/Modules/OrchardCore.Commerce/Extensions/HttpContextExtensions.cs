using System;

namespace Microsoft.AspNetCore.Http;

public static class HttpContextExtensions
{
    public static bool IsCheckoutFrontEnd(this HttpContext context) =>
        context.Request.Path.Value?.StartsWithOrdinalIgnoreCase("/checkout") == true ||
        context.Request.Path.Value?.StartsWithOrdinalIgnoreCase("/success") == true;

    public static bool IsCheckoutFrontEnd(this IHttpContextAccessor hca) =>
        hca.HttpContext is { } context && context.IsCheckoutFrontEnd();
}
