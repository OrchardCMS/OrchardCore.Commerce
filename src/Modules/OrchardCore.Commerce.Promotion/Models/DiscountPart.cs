using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Promotion.Models;

public class DiscountPart : ContentPart
{
    public NumericField DiscountPercentage { get; set; } = new();
    public PriceField DiscountAmount { get; set; }
    public DateTimeField BeginningUtc { get; set; } = new();
    public DateTimeField ExpirationUtc { get; set; } = new();
    public NumericField MaximumProducts { get; set; } = new();
    public NumericField MinimumProducts { get; set; } = new();
}
