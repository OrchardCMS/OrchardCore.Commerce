using OrchardCore.Commerce.Promotion.Models;

namespace OrchardCore.Commerce.Promotion.Extensions;

public static class DiscountPartExtensions
{
    public static bool IsValidAndActive(this DiscountPart discountPart) =>
        discountPart.DiscountPercentage?.Value is > 0 ^
        discountPart.DiscountAmount?.Amount is { IsValidAndNonZero: true };
}
