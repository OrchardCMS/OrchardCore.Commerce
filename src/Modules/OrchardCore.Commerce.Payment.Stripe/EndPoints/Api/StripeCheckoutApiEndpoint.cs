using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Environment.Shell;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Linq;
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
        [FromServices] ShellSettings shellSettings,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        //TODO: We need update also
        var customer = await stripeCustomerService.GetOrCreateCustomerAsync(
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
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "tenantName", shellSettings.Name },
                },
            },
        };

        foreach (var lineItem in options.LineItems)
        {
            //TODO: Change this to use the actual tax rate
            lineItem.TaxRates = ["txr_1F3586L1SJaDnrcsvfTTvknD"];
        }

        var session = await stripeSessionService.CreateSessionAsync(options);

        //Save session id to DB, with current User data and other necessary data
        var result = await stripeSessionService.SaveSessionDataAsync(customer, session);
        if (result.Errors?.Any() == true)
        {
            return TypedResults.BadRequest(result.Errors);
        }

        return TypedResults.Ok(session.Url);
    }
}
