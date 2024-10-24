using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripeProductPart : ContentPart
{
    public TextField StripeProductId { get; set; } = new();
    public IList<TextField> StripePriceIds { get; } = new List<TextField>();
}
