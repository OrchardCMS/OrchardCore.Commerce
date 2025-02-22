using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using Stripe;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class DummyStripeHelperService : IStripeHelperService
{
    // Set the event type to what you want to return.
    public static string Type { get; set; }
    public static IHasObject EventDataObject { get; set; }

    public Event PrepareStripeEvent(
        string json,
        string stripeSignatureHeader,
        string secret,
        bool throwOnApiVersionMismatch) =>
        new() { Id = "evt_exampleEventId0000000000", Type = Type, Data = new EventData { Object = EventDataObject } };
}
