using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public interface IStripeWebhookEventHandler
{
    Task ReceivedStripeEventAsync(Event stripeEvent);
}
