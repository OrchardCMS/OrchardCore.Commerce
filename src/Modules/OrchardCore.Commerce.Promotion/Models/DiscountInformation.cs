using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Promotion.Extensions;
using System;

namespace OrchardCore.Commerce.Promotion.Models;

public record DiscountInformation(
    decimal DiscountPercentage,
    Amount DiscountAmount,
    DateTime? BeginningUtc,
    DateTime? ExpirationUtc,
    decimal MaximumProducts,
    decimal MinimumProducts)
{
    public static explicit operator DiscountInformation(DiscountPart part) => new(
            part.DiscountPercentage?.Value ?? 0,
            part.DiscountAmount?.Amount ?? Amount.Unspecified,
            part.BeginningUtc.Value,
            part.ExpirationUtc.Value,
            part.MaximumProducts.Value ?? 0,
            part.MinimumProducts.Value ?? 0);

    public bool IsApplicable(int itemQuantity, DateTime? purchaseDateTime) =>
        this.IsValidAndActive() &&
        !(BeginningUtc > purchaseDateTime ||
          ExpirationUtc < purchaseDateTime ||
          MinimumProducts > itemQuantity ||
          (MaximumProducts > 0 && MaximumProducts < itemQuantity));
}
