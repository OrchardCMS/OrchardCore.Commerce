using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace OrchardCore.Commerce.Payment.Abstractions;

public static class PaymentEnvironmentHttpContextExtensions
{
    /// <summary>
    /// Resolves the payment environment from <paramref name="environment"/> or the <c>environment</c> query parameter.
    /// </summary>
    public static PaymentEnvironment GetPaymentEnvironment(this HttpContext httpContext, string? environment = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (string.IsNullOrWhiteSpace(environment))
        {
            environment = httpContext.Request.Query[PaymentEnvironmentExtensions.QueryParameterName];
        }

        return PaymentEnvironmentExtensions.Parse(environment);
    }

    /// <summary>
    /// Resolves a keyed payment client for the environment of this request.
    /// </summary>
    public static T GetRequiredKeyedPaymentService<T>(this HttpContext httpContext, string? environment = null)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        return httpContext.RequestServices.GetRequiredKeyedService<T>(httpContext.GetPaymentEnvironment(environment));
    }
}
