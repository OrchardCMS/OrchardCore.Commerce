namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripeSessionData
{
    public string UserId { get; set; }
    public string StripeCustomerId { get; set; }
    public string StripeSessionId { get; set; }
    public string StripeSessionUrl { get; set; }
    public string StripeInvoiceId { get; set; }
    public string SerializedAdditionalData { get; set; }
}
