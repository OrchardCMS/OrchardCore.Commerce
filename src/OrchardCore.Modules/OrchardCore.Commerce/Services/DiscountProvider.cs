using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
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
            ApplyPromotionToShoppingCartItem(item, purchaseDateTime)));

    public Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
        Task.FromResult(IsApplicable(model.Items.ToList(), model.PurchaseDateTime));

    private static bool IsApplicable(
        IList<PromotionAndTaxProviderContextLineItem> lineItems,
        DateTime? purchaseDateTime) =>
        lineItems.Any(item => IsApplicablePerItem(item, purchaseDateTime));

    private static bool IsApplicablePerItem(PromotionAndTaxProviderContextLineItem item, DateTime? purchaseDateTime)
    {
        var discountParts = item
            .Content
            .ContentItem?
            .OfType<DiscountPart>();

        return discountParts?.Any(discountPart => discountPart.IsApplicable(item.Quantity, purchaseDateTime)) == true;
    }

    public static PromotionAndTaxProviderContextLineItem ApplyPromotionToShoppingCartItem(
        PromotionAndTaxProviderContextLineItem item,
        DateTime? purchaseDateTime,
        IEnumerable<DiscountInformation> discountParts = null)
    {
        discountParts ??= item.Content.ContentItem.OfType<DiscountPart>().Select(part => (DiscountInformation)part);
        var newPrice = item.UnitPrice;

        var discountsUsed = new List<DiscountInformation>();
        foreach (var discount in discountParts)
        {
            if (!discount.IsApplicable(item.Quantity, purchaseDateTime)) continue;

            var discountPercentage = discount.DiscountPercentage;
            var discountAmount = discount.DiscountAmount;

            if (discountPercentage > 0)
            {
                newPrice = newPrice.WithDiscount(discountPercentage);
            }
            else if (discountAmount.IsValidAndNonZero)
            {
                newPrice = newPrice.WithDiscount(discountAmount);
            }
            else
            {
                continue;
            }

            discountsUsed.Add(discount);
        }

        return item with { UnitPrice = newPrice, Discounts = discountsUsed };
    }
}
