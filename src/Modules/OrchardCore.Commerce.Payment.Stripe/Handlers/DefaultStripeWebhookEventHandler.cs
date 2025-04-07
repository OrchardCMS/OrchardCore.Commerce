using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using Stripe;
using System.Threading.Tasks;
using static Stripe.EventTypes;

namespace OrchardCore.Commerce.Payment.Stripe.Handlers;

public class DefaultStripeWebhookEventHandler : IStripeWebhookEventHandler
{
    private readonly IStripePaymentIntentService _stripePaymentIntentService;
    private readonly IStripePaymentService _stripePaymentService;

    public DefaultStripeWebhookEventHandler(
        IStripePaymentIntentService stripePaymentIntentService,
        IStripePaymentService stripePaymentService)
    {
        _stripePaymentIntentService = stripePaymentIntentService;
        _stripePaymentService = stripePaymentService;
    }

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

            var paymentIntent = await _stripePaymentIntentService.GetPaymentIntentAsync(paymentIntentId);
            await _stripePaymentService.UpdateOrderToOrderedAsync(paymentIntent, shoppingCartId: null);
        }
        else if (stripeEvent.Type == PaymentIntentPaymentFailed)
        {
            var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
            await _stripePaymentService.UpdateOrderToPaymentFailedAsync(paymentIntent.Id);
        }
    }
}
