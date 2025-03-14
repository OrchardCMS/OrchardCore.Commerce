using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripeProductPart : ContentPart
{
    public TextField StripeProductId { get; set; } = new();
}
