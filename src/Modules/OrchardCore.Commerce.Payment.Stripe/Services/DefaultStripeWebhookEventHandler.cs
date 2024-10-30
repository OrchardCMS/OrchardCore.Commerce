using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using Stripe;
using System.Threading.Tasks;
using static Stripe.Events;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class DefaultStripeWebhookEventHandler : IStripeWebhookEventHandler
{
    private readonly IStripePaymentService _stripePaymentService;

    public DefaultStripeWebhookEventHandler(IStripePaymentService stripePaymentService) =>
        _stripePaymentService = stripePaymentService;

    public async Task ReceivedStripeEventAsync(Event stripeEvent)
    {
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

            var paymentIntent = await _stripePaymentService.GetPaymentIntentAsync(paymentIntentId);
            await _stripePaymentService.UpdateOrderToOrderedAsync(paymentIntent, shoppingCartId: null);
        }
        else if (stripeEvent.Type == PaymentIntentPaymentFailed)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            await _stripePaymentService.UpdateOrderToPaymentFailedAsync(paymentIntent.Id);
        }
    }
}
