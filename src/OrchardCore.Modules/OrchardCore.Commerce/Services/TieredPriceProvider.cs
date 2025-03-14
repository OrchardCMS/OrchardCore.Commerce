using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A price provider that obtains from a product by looking for a `PriceVariantsPart` based on the quantity.
/// </summary>
public class TieredPriceProvider : IPriceProvider
{
    private readonly IProductService _productService;
    private readonly IMoneyService _moneyService;

    public int Order => 1;

    public TieredPriceProvider(IProductService productService, IMoneyService moneyService)
    {
        _productService = productService;
        _moneyService = moneyService;
    }

    public async Task<IList<ShoppingCartItem>> UpdateAsync(IList<ShoppingCartItem> model)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(model);

        return model
            .Select(item =>
                skuProducts.TryGetValue(item.ProductSku, out var productPart) &&
                    AddPriceToShoppingCartItem(item, productPart) is { } itemWithPrice
                        ? itemWithPrice
                        : item)
            .ToList();
    }

    private ShoppingCartItem AddPriceToShoppingCartItem(ShoppingCartItem item, ProductPart productPart)
    {
        if (productPart.ContentItem.As<TieredPricePart>() is not { } tieredPricePart) return null;

        return item.WithPrice(
            new PrioritizedPrice(1, tieredPricePart.GetPriceForQuantity(_moneyService, item.Quantity)));
    }

    public async Task<bool> IsApplicableAsync(IList<ShoppingCartItem> model)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(model);

        return model.All(item =>
                skuProducts.TryGetValue(item.ProductSku, out var productPart) &&
                productPart.ContentItem.Has<TieredPricePart>());
    }
}
