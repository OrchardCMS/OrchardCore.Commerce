using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Promotion.Extensions;

public static class DiscountPartExtensions
{
    public static bool IsValidAndActive(this DiscountInformation discount) =>
        discount.DiscountPercentage > 0 ^
        discount.DiscountAmount is { IsValidAndNonZero: true };

    public static bool IsValidAndActive(this DiscountPart discountPart) =>
        ((DiscountInformation)discountPart).IsValidAndActive();

    public static IEnumerable<DiscountPart> GetAllDiscountParts(this IContent content) =>
        content
            .ContentItem
            .OfType<DiscountPart>();

    public static IEnumerable<DiscountInformation> GetAllDiscountInformation(this IContent content) =>
        content
            .GetAllDiscountParts()
            .Select(part => (DiscountInformation)part);
}
