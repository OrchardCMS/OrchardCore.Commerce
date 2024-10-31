#nullable enable
using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Payment.Stripe.Endpoints.Constants.Endpoints;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;
public static class StripeConfirmationTokenEndpoint
{
    public static IEndpointRouteBuilder AddStripeConfirmationTokenEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGetWithDefaultSettings($"{StripePaymentApiPath}/confirmation-token", GetStripeConfirmationTokenAsync);
        return builder;
    }

    private static async Task<IResult> GetStripeConfirmationTokenAsync(
        [FromQuery] string? confirmationTokenId,
        [FromServices] IStripeConfirmationTokenService stripeConfirmationTokenService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var confirmationToken = await stripeConfirmationTokenService.GetConfirmationTokenAsync(confirmationTokenId);
        return TypedResults.Ok(confirmationToken);
    }
}
