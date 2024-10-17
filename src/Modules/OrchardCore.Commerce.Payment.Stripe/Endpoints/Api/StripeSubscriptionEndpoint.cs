#nullable enable
using Lombiq.HelpfulLibraries.AspNetCore.Extensions;
using Lombiq.HelpfulLibraries.OrchardCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Endpoints;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using OrchardCore.Commerce.Payment.Stripe.Extensions;
using OrchardCore.Commerce.Payment.Stripe.ViewModels;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OrchardCore.Commerce.Payment.Stripe.Endpoints.Constants.Endpoints;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;
public static class StripeSubscriptionEndpoint
{
    public static IEndpointRouteBuilder AddStripeCreateSubscriptionEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPostWithDefaultSettings($"{StripePaymentApiPath}/subscription", GetStripeCreateSubscriptionAsync);
        return builder;
    }

    private static async Task<IResult> GetStripeCreateSubscriptionAsync(
        [FromBody] StripeCreateSubscriptionViewModel viewModel,
        [FromServices] IStripePaymentService stripePaymentService,
        [FromServices] IShoppingCartService shoppingCartService,
        [FromServices] IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, ApiPermissions.CommerceApiStripePayment))
        {
            return httpContext.ChallengeOrForbidApi();
        }

        // Get price IDs from the shopping cart.
        var shoppingCartViewModel = await shoppingCartService.GetAsync(viewModel.ShoppingCartId);
        var priceIds = shoppingCartViewModel.Lines.SelectMany(line => line.AdditionalData.GetPriceIds()).ToList();
        viewModel.PriceIds.AddRange(priceIds);

        // Create customer if it doesn't exist.
        if (string.IsNullOrEmpty(viewModel.CustomerId))
        {
            var orderPart = viewModel.OrderPart;
            var shippingAddress = orderPart.ShippingAddress.Address;
            var billingAddress = orderPart.BillingAddress.Address;
            var options = new CustomerCreateOptions
            {
                Email = orderPart.Email.Text,
                Name = orderPart.BillingAddress.Address.Name,
                Shipping = string.IsNullOrEmpty(shippingAddress.Name)
                    ? null
                    : new ShippingOptions
                    {
                        Address = new AddressOptions
                        {
                            City = shippingAddress.City,
                            Country = shippingAddress.Region,
                            Line1 = shippingAddress.StreetAddress1,
                            Line2 = shippingAddress.StreetAddress2,
                            PostalCode = shippingAddress.PostalCode,
                            State = shippingAddress.Province,
                        },
                        Name = shippingAddress.Name,
                    },
                Address = new AddressOptions
                {
                    City = billingAddress.City,
                    Country = billingAddress.Region,
                    Line1 = billingAddress.StreetAddress1,
                    Line2 = billingAddress.StreetAddress2,
                    PostalCode = billingAddress.PostalCode,
                    State = billingAddress.Province,
                },
            };
            var customer = await stripePaymentService.CreateCustomerAsync(options);
            viewModel.CustomerId = customer.Id;
        }

        // Create the subscription.
        var subscription = await stripePaymentService.CreateSubscriptionAsync(viewModel);
        return TypedResults.Ok(subscription);
    }
}
