using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripeProductFeaturePart : ContentPart
{
    public TextField FeatureName { get; set; } = new();
}
