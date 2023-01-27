using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class DiscountProvider : IPromotionProvider
{
    public int Order => 0;

    public Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model) =>
        model.UpdateAsync((item, purchaseDateTime) => Task.FromResult(
            ApplyPromotionToShoppingCartItem(item, purchaseDateTime, item.Content.ContentItem.OfType<DiscountPart>())));

    public Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
        Task.FromResult(IsApplicable(model.Items.ToList(), model.PurchaseDateTime));

    private static bool IsApplicable(
        IList<PromotionAndTaxProviderContextLineItem> lineItems,
        DateTime? purchaseDateTime) =>
        lineItems.All(item => IsApplicablePerItem(item, purchaseDateTime));

    private static bool IsApplicablePerItem(PromotionAndTaxProviderContextLineItem item, DateTime? purchaseDateTime)
    {
        var discountParts = item.Content
            .ContentItem
            .OfType<DiscountPart>();

        return discountParts.Any(discountPart => discountPart.IsApplicable(item.Quantity, purchaseDateTime));
    }

    public static Amount ApplyPromotionToShoppingCartItem(
        PromotionAndTaxProviderContextLineItem item,
        DateTime? purchaseDateTime,
        IEnumerable<DiscountPart> discountParts)
    {
        var newPrice = item.UnitPrice;

        foreach (var discountPart in discountParts)
        {
            if (!discountPart.IsApplicable(item.Quantity, purchaseDateTime)) continue;

            var discountPercentage = discountPart.DiscountPercentage?.Value;
            var discountAmount = discountPart.DiscountAmount.Amount;

            if (discountPercentage is { } and not 0)
            {
                newPrice = newPrice.WithDiscount((decimal)discountPercentage);
            }

            if (discountAmount.IsValidAndNonZero)
            {
                newPrice = newPrice.WithDiscount(discountAmount);
            }
        }

        return newPrice;
    }
}
