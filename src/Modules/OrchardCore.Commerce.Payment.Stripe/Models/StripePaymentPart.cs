using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripePaymentPart : ContentPart
{
    public TextField PaymentIntentId { get; set; } = new();
    public int RetryCounter { get; set; }
}
