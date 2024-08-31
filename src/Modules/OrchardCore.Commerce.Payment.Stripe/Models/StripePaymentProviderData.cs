namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripePaymentProviderData
{
    public string PublishableKey { get; set; }
    public string ClientSecret { get; set; }
    public string PaymentIntentId { get; set; }
}
