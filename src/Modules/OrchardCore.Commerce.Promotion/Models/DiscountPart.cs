using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Promotion.Models;

public class DiscountPart : ContentPart
{
    public NumericField Percentage { get; set; } = new();
    public Amount Amount { get; set; }
    public DateTimeField BeginningUtc { get; set; } = new();
    public DateTimeField ExpirationUtc { get; set; } = new();
    public NumericField Maximum { get; set; } = new();
    public NumericField Minimum { get; set; } = new();
}
