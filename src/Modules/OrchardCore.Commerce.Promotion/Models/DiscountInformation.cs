using OrchardCore.Commerce.MoneyDataType;
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
    public DiscountInformation(DiscountPart part)
        : this(
            part.DiscountPercentage?.Value ?? 0,
            part.DiscountAmount?.Amount ?? Amount.Unspecified,
            part.BeginningUtc.Value,
            part.ExpirationUtc.Value,
            part.MaximumProducts.Value ?? 0,
            part.MinimumProducts.Value ?? 0)
    {
    }

    public static implicit operator DiscountInformation(DiscountPart part) => new(part);
}
