using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Helpers;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A simple price provider that obtains a price from a product by looking for a `PriceVariantsPart`.
/// </summary>
public class PriceVariantProvider : IPriceProvider
{
    private readonly IProductService _productService;
    private readonly IPredefinedValuesProductAttributeService _predefinedValuesService;

    public int Order => 1;

    public PriceVariantProvider(IProductService productService, IPredefinedValuesProductAttributeService predefinedValuesService)
    {
        _productService = productService;
        _predefinedValuesService = predefinedValuesService;
    }

    public async Task<IEnumerable<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(items);

        return items
            .Select(item =>
            {
                if (skuProducts.TryGetValue(item.ProductSku, out var productPart))
                {
                    var itemWithPrice = AddPriceToShoppingCartItem(item, productPart);

                    if (itemWithPrice != null)
                    {
                        return itemWithPrice;
                    }
                }

                return item;
            });
    }

    public async Task<ShoppingCartItem> AddPriceAsync(ShoppingCartItem item)
    {
        var sku = item.ProductSku;
        var productPart = await _productService.GetProductAsync(sku);
        var productPartSku = productPart.Sku;

        if (productPartSku == sku)
        {
            var itemWithPrice = AddPriceToShoppingCartItem(item, productPart);

            if (itemWithPrice != null)
            {
                return itemWithPrice;
            }
        }

        return item;
    }

    private ShoppingCartItem AddPriceToShoppingCartItem(ShoppingCartItem item, ProductPart productPart)
    {
        var priceVariantsPart = productPart.ContentItem.As<PriceVariantsPart>();

        if (priceVariantsPart is { Variants: { } })
        {
            var attributesRestrictedToPredefinedValues = _predefinedValuesService
                .GetProductAttributesRestrictedToPredefinedValues(productPart.ContentItem)
                .Select(attr => attr.PartName + "." + attr.Name)
                .ToHashSet();

            var predefinedAttributes = item.Attributes
                .OfType<IPredefinedValuesProductAttributeValue>()
                .Where(attribute => attributesRestrictedToPredefinedValues.Contains(attribute.AttributeName))
                .OrderBy(value => value.AttributeName);

            var variantKey = string.Join(
                "-",
                predefinedAttributes
                    .Select(attr => attr.UntypedPredefinedValue)
                    .Where(value => value != null));

            if (priceVariantsPart.Variants.ContainsKey(variantKey))
            {
                return item.WithPrice(new PrioritizedPrice(1, priceVariantsPart.Variants[variantKey]));
            }
        }

        return null;
    }

    public async Task<bool> IsApplicableAsync(IList<ShoppingCartItem> items)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(items);

        return items
            .All(item =>
            {
                if (skuProducts.TryGetValue(item.ProductSku, out var productPart))
                {
                    var contentItem = productPart.ContentItem;
                    return contentItem.OfType<PricePart>().Any() || contentItem.OfType<PriceVariantsPart>().Any();
                }

                return false;
            });
    }
}
