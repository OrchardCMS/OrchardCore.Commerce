using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
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

    public async Task<IEnumerable<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items)
    {
        var skus = items.Select(item => item.ProductSku).Distinct().ToArray();
        var skuProducts = (await _productService.GetProductsAsync(skus)).ToDictionary(productPart => productPart.Sku);
        return items
            .Select(item =>
            {
                if (skuProducts.TryGetValue(item.ProductSku, out var productPart))
                {
                    return AddPriceToShoppingCartItem(item, productPart);
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
            return AddPriceToShoppingCartItem(item, productPart);
        }

        return item;
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
