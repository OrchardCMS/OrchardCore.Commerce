using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using Stripe.Checkout;
using System.Threading.Tasks;

using static OrchardCore.Commerce.Payment.Stripe.Endpoints.Constants.Endpoints;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;

public static class StripeCheckoutApiEndpoint
{
    public static IEndpointRouteBuilder AddStripeCheckoutEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings($"{StripePaymentApiPath}/checkout-session", GetStripeCheckoutEndpointAsync);
        return builder;
    }

    private static async Task<IResult> GetStripeCheckoutEndpointAsync(
        [FromBody] SubscriptionCheckoutEndpointViewModel viewModel,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IStripeCustomerService stripeCustomerService,
        [FromServices] IStripeSessionService stripeSessionService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var customer = await stripeCustomerService.GetAndUpdateOrCreateCustomerAsync(
            viewModel.BillingAddress,
            viewModel.ShippingAddress,
            viewModel.Email,
            viewModel.Phone);

        var mode = viewModel.PaymentMode == PaymentMode.Payment ? "payment" : "subscription";
        var options = new SessionCreateOptions
        {
            LineItems = [.. viewModel.SessionLineItemOptions],
            Mode = mode,
            SuccessUrl = viewModel.SuccessUrl,
            CancelUrl = viewModel.CancelUrl,
            Customer = customer.Id,
        };

        var session = await stripeSessionService.CreateSessionAsync(options);

        return TypedResults.Ok(session.Url);
    }
}
