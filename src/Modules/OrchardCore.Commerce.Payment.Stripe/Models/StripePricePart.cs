using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Payment.Stripe.Models;

public class StripePricePart : ContentPart
{
    public TextField PriceId { get; set; } = new();
    public TextField Name { get; set; } = new();
    public PriceField Price { get; set; } = new();
    public TextField Period { get; set; } = new();
}
