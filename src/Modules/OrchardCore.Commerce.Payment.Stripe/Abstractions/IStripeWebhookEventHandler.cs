using Stripe;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Abstractions;

/// <summary>
/// Event handler for the Stripe webhook.
/// </summary>
public interface IStripeWebhookEventHandler
{
    /// <summary>
    /// Called when a Stripe event is received. This is where you can handle the event.
    /// </summary>
    /// <param name="stripeEvent">Contains the Stripe Event parameters.</param>
    Task ReceivedStripeEventAsync(Event stripeEvent);
}
