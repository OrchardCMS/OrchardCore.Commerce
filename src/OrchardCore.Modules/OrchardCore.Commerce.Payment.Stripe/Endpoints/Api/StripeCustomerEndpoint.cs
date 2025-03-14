using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using Stripe;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Payment.Stripe.Endpoints.Constants.Endpoints;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;
public static class StripeCustomerEndpoint
{
    public static IEndpointRouteBuilder AddStripeGetCustomerEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGetWithDefaultSettings($"{StripePaymentApiPath}/customer", GetStripeCustomerAsync);
        return builder;
    }

    private static async Task<IResult> GetStripeCustomerAsync(
        [FromQuery] string customerId,
        [FromServices] IStripeCustomerService stripeCustomerService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var customer = await stripeCustomerService.GetCustomerByIdAsync(customerId);
        return TypedResults.Ok(customer);
    }

    public static IEndpointRouteBuilder AddStripeCreateCustomerEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings($"{StripePaymentApiPath}/customer", GetStripeCreateCustomerAsync);
        return builder;
    }

    private static async Task<IResult> GetStripeCreateCustomerAsync(
        [FromBody] CustomerCreateOptions customerCreateOptions,
        [FromServices] IStripeCustomerService stripeCustomerService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var customer = await stripeCustomerService.CreateCustomerAsync(customerCreateOptions);
        return TypedResults.Ok(customer);
    }
}
