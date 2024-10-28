using Azure;
using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Models;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using OrchardCore.ContentManagement;
using Stripe;
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

    // Try to get customer, if not exists, create it
    private static async Task<Customer> GetOrCreateCustomerAsync(
        SubscriptionCheckoutEndpointViewModel viewModel,
        HttpContext httpContext,
        IRequestOptionsService requestOptionsService)
    {
        if (string.IsNullOrEmpty(viewModel.CustomerId))
        {
            return await CreateCustomerAsync(viewModel, httpContext, requestOptionsService);
        }

        var customerService = new CustomerService();
        var customer = await customerService.GetAsync(
            viewModel.CustomerId,
            requestOptions: await requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: httpContext.RequestAborted);

        if (customer?.Id != null)
        {
            return customer;
        }

        return await CreateCustomerAsync(viewModel, httpContext, requestOptionsService);
    }

    private static async Task<Customer> CreateCustomerAsync(
        SubscriptionCheckoutEndpointViewModel viewModel,
        HttpContext httpContext,
        IRequestOptionsService requestOptionsService)
    {
        var billingAddress = viewModel.Information.BillingAddress.Address;
        var shippingAddress = viewModel.Information.ShippingAddress.Address;
        var customerCreateOptions = new CustomerCreateOptions
        {
            Name = billingAddress.Name,
            Email = viewModel.Information.Email.Text,
            Phone = viewModel.Information.Phone.Text,
            Address = new AddressOptions
            {
                City = billingAddress.City,
                Country = billingAddress.Region,
                Line1 = billingAddress.StreetAddress1,
                Line2 = billingAddress.StreetAddress2,
                PostalCode = billingAddress.PostalCode,
                State = billingAddress.Province,
            },
            Shipping = shippingAddress.Name != null
                ? new ShippingOptions
                    {
                        Name = shippingAddress.Name,
                        Address = new AddressOptions
                        {
                            City = shippingAddress.City,
                            Country = shippingAddress.Region,
                            Line1 = shippingAddress.StreetAddress1,
                            Line2 = shippingAddress.StreetAddress2,
                            PostalCode = shippingAddress.PostalCode,
                            State = shippingAddress.Province,
                        },
                    }
                : null,
        };

        var customerService = new CustomerService();
        var customer = await customerService.CreateAsync(
            customerCreateOptions,
            await requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: httpContext.RequestAborted);

        //TODO: Save customer id to DB, with current User data and other necessary data

        return customer;
    }

    private static async Task<IResult> GetStripeCheckoutEndpointAsync(
        [FromBody] SubscriptionCheckoutEndpointViewModel viewModel,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IRequestOptionsService requestOptionsService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        var customer = await GetOrCreateCustomerAsync(viewModel, httpContext, requestOptionsService);
        var mode = viewModel.PaymentMode == PaymentMode.Payment ? "payment" : "subscription";

        var options = new SessionCreateOptions
        {
            LineItems = [.. viewModel.SessionLineItemOptions],
            Mode = mode,
            Discounts = new List<SessionDiscountOptions>
            {
                new SessionDiscountOptions
                {
                    Coupon = "QTpgGHha"
                },
            },
            SuccessUrl = viewModel.SuccessUrl,
            CancelUrl = viewModel.CancelUrl,
            Customer = customer.Id,
        };

        var service = new SessionService();
        var session = await service.CreateAsync(
            options,
            await requestOptionsService.SetIdempotencyKeyAsync(),
            cancellationToken: httpContext.RequestAborted);

        //Save session id to DB, with current User data and other necessary data

        return TypedResults.Ok(session.Url);
    }
}
