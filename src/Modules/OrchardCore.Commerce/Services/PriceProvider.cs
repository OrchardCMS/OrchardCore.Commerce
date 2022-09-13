using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A simple price provider that obtains a price from a product by looking for a `PricePart`.
/// </summary>
public class PriceProvider : IPriceProvider
{
    private readonly IProductService _productService;
    private readonly IMoneyService _moneyService;

    public int Order => 0;

    public PriceProvider(
        IProductService productService,
        IMoneyService moneyService)
    {
        _productService = productService;
        _moneyService = moneyService;
    }

    public async Task<IList<ShoppingCartItem>> UpdateAsync(IList<ShoppingCartItem> model)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(model);

        return model
            .Select(item => skuProducts.TryGetValue(item.ProductSku, out var productPart)
                ? AddPriceToShoppingCartItem(item, productPart)
                : item)
            .ToList();
    }

    public async Task<bool> IsApplicableAsync(IList<ShoppingCartItem> model)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(model);

        return model.All(item =>
            skuProducts.TryGetValue(item.ProductSku, out var productPart) &&
            productPart.ContentItem.Has<PricePart>());
    }

    private ShoppingCartItem AddPriceToShoppingCartItem(ShoppingCartItem item, ProductPart productPart)
    {
        var newPrices = productPart
            .ContentItem
            .OfType<PricePart>()
            .Where(pricePart => pricePart.Price.Currency.Equals(_moneyService.CurrentDisplayCurrency))
            .Select(pricePart => new PrioritizedPrice(0, pricePart.Price));
        return item.WithPrices(newPrices);
    }
}
