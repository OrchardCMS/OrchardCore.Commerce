using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Extensions;
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
    private readonly IShoppingCartSerializer _shoppingCartSerializer;

    public int Order => 1;

    public PriceVariantProvider(
        IProductService productService,
        IPredefinedValuesProductAttributeService predefinedValuesService,
        IShoppingCartSerializer shoppingCartSerializer)
    {
        _productService = productService;
        _predefinedValuesService = predefinedValuesService;
        _shoppingCartSerializer = shoppingCartSerializer;
    }

    public async Task<IList<ShoppingCartItem>> UpdateAsync(IList<ShoppingCartItem> model)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(model);

        var updatedModel = await Task.WhenAll(model
            .Select(async item =>
            {
                if (skuProducts.TryGetValue(item.ProductSku, out var productPart))
                {
                    var itemWithPrice = await AddPriceToShoppingCartItemAsync(item, productPart);

                    if (itemWithPrice != null)
                    {
                        return itemWithPrice;
                    }
                }

                return item;
            }));

        return [.. updatedModel];
    }

    private async Task<ShoppingCartItem> AddPriceToShoppingCartItemAsync(ShoppingCartItem item, ProductPart productPart)
    {
        var priceVariantsPart = productPart.ContentItem.As<PriceVariantsPart>();

        if (priceVariantsPart is { Variants: { } variants } && variants.Any())
        {
            var attributesRestrictedToPredefinedValues = (await _predefinedValuesService
                .GetProductAttributesRestrictedToPredefinedValuesAsync(productPart.ContentItem))
                .Select(attr => attr.PartName + "." + attr.Name)
                .ToHashSet();

            if (item.HasRawAttributes())
            {
                item.Attributes.SetItems(await _shoppingCartSerializer.PostProcessAttributesAsync(item.Attributes, productPart));
            }

            var key = item.GetVariantKeyFromAttributes(attributesRestrictedToPredefinedValues);

            if (string.IsNullOrEmpty(key)) return item.WithPrice(new PrioritizedPrice(1, variants.First().Value));
            if (variants.TryGetValue(key, out var variant)) return item.WithPrice(new PrioritizedPrice(1, variant));
        }

        return null;
    }

    public async Task<bool> IsApplicableAsync(IList<ShoppingCartItem> model)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(model);

        return model.All(item =>
                skuProducts.TryGetValue(item.ProductSku, out var productPart) &&
                productPart.ContentItem.Has<PriceVariantsPart>());
    }
}
