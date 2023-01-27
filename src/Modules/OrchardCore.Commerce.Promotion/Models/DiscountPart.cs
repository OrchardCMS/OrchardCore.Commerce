using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System;

namespace OrchardCore.Commerce.Promotion.Models;

public class DiscountPart : ContentPart
{
    public NumericField DiscountPercentage { get; set; } = new();
    public PriceField DiscountAmount { get; set; } = new();
    public DateTimeField BeginningUtc { get; set; } = new();
    public DateTimeField ExpirationUtc { get; set; } = new();
    public NumericField MaximumProducts { get; set; } = new();
    public NumericField MinimumProducts { get; set; } = new();

    public bool IsApplicable(int itemQuantity, DateTime? purchaseDateTime)
    {
        var discountMaximumProducts = MaximumProducts.Value;

        return this.IsValidAndActive() &&
               !(BeginningUtc.Value > purchaseDateTime ||
                 ExpirationUtc.Value < purchaseDateTime ||
                 MinimumProducts.Value > itemQuantity ||
                 (discountMaximumProducts > 0 && discountMaximumProducts < itemQuantity));
    }
}
