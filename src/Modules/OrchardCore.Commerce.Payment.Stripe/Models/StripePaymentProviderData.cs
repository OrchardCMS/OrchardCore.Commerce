#nullable enable

namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripePaymentProviderData
{
    public required string PublishableKey { get; set; }
    public required string ClientSecret { get; set; }
    public string? AccountId { get; set; }
    public required string PaymentIntentId { get; set; }
}
