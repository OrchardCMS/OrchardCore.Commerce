using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Extensions;

public static class Endpoints
{
    public static IEndpointRouteBuilder AddStripePaymentApiEndpoints(this IEndpointRouteBuilder router)
    {
        router
            .AddStripeConfirmationTokenEndpoint()
            .AddStripePublicKeyEndpoint()
            .AddStripePaymentIntentPostEndpoint()
            .AddStripePaymentIntentGetEndpoint()
            .AddStripeTotalEndpoint()
            .AddStripeConfirmParametersEndpoint()
            .AddStripePaymentOrderConfirmationEndpoint()
            .AddStripeGetCustomerEndpoint()
            .AddStripeCreateCustomerEndpoint()
            .AddStripeCreateSubscriptionEndpoint();

        return router;
    }
}
