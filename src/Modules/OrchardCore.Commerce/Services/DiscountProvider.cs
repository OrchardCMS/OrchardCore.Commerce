using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Extensions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A discount provider that obtains a discount from a product by looking for a `DiscountPart`.
/// </summary>
public class DiscountProvider : IPromotionProvider
{
    private readonly IProductService _productService;

    public int Order => 0;

    public DiscountProvider(IProductService productService) =>
        _productService = productService;

    public Task<PromotionAndTaxProviderContext> UpdateAsync(PromotionAndTaxProviderContext model)
    {
        var items = model.Items.AsList();

        var newContextLineItems =
            items.Select(item => item with { UnitPrice = ApplyPromotionToShoppingCartItem(item) });

        var updatedTotals = model
            .TotalsByCurrency
            .Select(total =>
            {
                var currency = total.Currency.CurrencyIsoCode;
                return newContextLineItems
                    .Where(item => item.Subtotal.Currency.CurrencyIsoCode == currency)
                    .Select(item => item.Subtotal)
                    .Sum();
            });

        return Task.FromResult(new PromotionAndTaxProviderContext(newContextLineItems, updatedTotals));
    }

    public Task<bool> IsApplicableAsync(PromotionAndTaxProviderContext model) =>
    Task.FromResult(IsApplicable(model.Items.ToList()));

    private static bool IsApplicable(IList<PromotionAndTaxProviderContextLineItem> lineItems) =>

        lineItems.Any(item => IsApplicablePerItem(item));

    private static bool IsApplicablePerItem(PromotionAndTaxProviderContextLineItem item)
    {
        var discountParts = item.Content
            .ContentItem
            .OfType<DiscountPart>();

        return discountParts.Any(discountPart => IsApplicablePerDiscountPart(discountPart, item.Quantity));
    }

    // In-case we have multiple discount parts on one product.
    private static bool IsApplicablePerDiscountPart(DiscountPart discountPart, int itemQuantity)
    {
        var discountMaximumProducts = discountPart.MaximumProducts.Value;

        var discountPresent = discountPart.DiscountPercentage?.Value is { } and not 0 ||
            (discountPart.DiscountAmount?.Amount is { } notNullDiscountAmount &&
            IsValidAndNotZero(notNullDiscountAmount));

        return discountPresent &&
               !(discountPart.BeginningUtc.Value > DateTime.UtcNow ||
               discountPart.ExpirationUtc.Value < DateTime.UtcNow ||
               discountPart.MinimumProducts.Value > itemQuantity ||
               (discountMaximumProducts > 0 && discountMaximumProducts < itemQuantity));
    }

    private static Amount ApplyPromotionToShoppingCartItem(PromotionAndTaxProviderContextLineItem item)
    {
        var newPrice = item.UnitPrice;

        var discountParts = item.Content
            .ContentItem
            .OfType<DiscountPart>();

        foreach (var discountPart in discountParts)
        {
            if (!IsApplicablePerDiscountPart(discountPart, item.Quantity)) continue;

            var discountPercentage = discountPart.DiscountPercentage?.Value;
            var discountAmount = discountPart.DiscountAmount?.Amount;

            if (discountPercentage is { } and not 0)
            {
                newPrice =
                    new Amount(
                        Math.Round(newPrice.Value * (1 - ((decimal)discountPercentage / 100)), 2),
                        newPrice.Currency);
            }

            if (discountAmount is { } notNullDiscountAmount &&
                IsValidAndNotZero(notNullDiscountAmount))
            {
                newPrice =
                    new Amount(
                        Math.Round(Math.Max(0, newPrice.Value - notNullDiscountAmount.Value), 2),
                        newPrice.Currency);
            }
        }

        return newPrice;
    }

    private static bool IsValidAndNotZero(Amount? amount) =>
        amount is { } notNullAmount && notNullAmount.IsValid && notNullAmount.Value != 0;
}
