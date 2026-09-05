using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using Stripe;
using System;
using System.Threading.Tasks;
using static Stripe.EventTypes;

namespace OrchardCore.Commerce.Payment.Stripe.Handlers;

public class DefaultStripeWebhookEventHandler : IStripeWebhookEventHandler
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultStripeWebhookEventHandler(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public async Task ReceivedStripeEventAsync(Event stripeEvent, PaymentEnvironment environment)
    {
        var stripePaymentIntentService = _serviceProvider.GetRequiredKeyedService<IStripePaymentIntentService>(environment);
        var stripePaymentService = _serviceProvider.GetRequiredKeyedService<IStripePaymentService>(environment);

        if (stripeEvent.Type == ChargeSucceeded)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge?.PaymentIntentId is not { } paymentIntentId)
            {
                return;
            }

            // If the charge is associated with a customer, it means it's a subscription payment in the current implementation.
            if (!string.IsNullOrEmpty(charge.CustomerId))
            {
                return;
            }

            var paymentIntent = await stripePaymentIntentService.GetPaymentIntentAsync(paymentIntentId);
            await stripePaymentService.UpdateOrderToOrderedAsync(paymentIntent, shoppingCartId: null);
        }
        else if (stripeEvent.Type == PaymentIntentPaymentFailed)
        {
            var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
            await stripePaymentService.UpdateOrderToPaymentFailedAsync(paymentIntent.Id);
        }
    }
}
