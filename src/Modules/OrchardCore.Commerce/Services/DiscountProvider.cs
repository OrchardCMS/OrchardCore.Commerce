using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
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

    public async Task<IList<ShoppingCartItem>> UpdateAsync(IList<ShoppingCartItem> model)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(model);

        return model
            .Select(item => skuProducts.TryGetValue(item.ProductSku, out var productPart)
                ? ApplyPromotionToShoppingCartItem(item, productPart)
                : item)
            .ToList();
    }

    public Task<bool> IsApplicableAsync(IList<ShoppingCartItem> model) => Task.FromResult(true);

    private static ShoppingCartItem ApplyPromotionToShoppingCartItem(ShoppingCartItem item, ProductPart productPart)
    {
        var itemPrices = item.Prices;
        var itemQuantity = item.Quantity;

        var discountParts = productPart
            .ContentItem
            .OfType<DiscountPart>();

        IList<PrioritizedPrice> newPrices = new List<PrioritizedPrice>();

        foreach (var discountPart in discountParts)
        {
            var discountMaximumProducts = discountPart.MaximumProducts.Value;

            var discountPercentage = discountPart.DiscountPercentage?.Value;
            var discountAmount = discountPart.DiscountAmount?.Amount;

            if (discountPart.BeginningUtc.Value > DateTime.UtcNow ||
                discountPart.ExpirationUtc.Value < DateTime.UtcNow ||
                discountPart.MinimumProducts.Value > itemQuantity ||
                (discountMaximumProducts > 0 && discountMaximumProducts < itemQuantity))
            {
                newPrices.AddRange(itemPrices);
                continue;
            }

            if (discountPercentage is { } and not 0)
            {
                newPrices.AddRange(itemPrices.Select(prioritizedPrice =>
                    new PrioritizedPrice(
                        prioritizedPrice.Priority,
                        new Amount(
                            Math.Round(prioritizedPrice.Price.Value * (1 - ((decimal)discountPercentage / 100)), 2),
                            prioritizedPrice.Price.Currency))));
            }

            if (discountAmount is { } notNullDiscountAmount &&
                notNullDiscountAmount.IsValid &&
                notNullDiscountAmount.Value != 0)
            {
                newPrices.AddRange(itemPrices.Select(prioritizedPrice =>
                    new PrioritizedPrice(
                        prioritizedPrice.Priority,
                        new Amount(
                            Math.Round(Math.Max(0, prioritizedPrice.Price.Value - notNullDiscountAmount.Value), 2),
                            prioritizedPrice.Price.Currency))));
            }
        }

        return item.WithPrices(newPrices);
    }
}
