using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A simple price provider that obtains a price from a product by looking for a `PricePart`
    /// </summary>
    public class PriceProvider : IPriceProvider
    {
        private readonly IProductService _productService;
        private readonly IMoneyService _moneyService;

        public PriceProvider(
            IProductService productService,
            IMoneyService moneyService)
        {
            _productService = productService;
            _moneyService = moneyService;
        }

        public int Order => 0;

        public async Task AddPrices(IList<ShoppingCartItem> items)
        {
            var skus = items.Select(item => item.ProductSku).Distinct().ToArray();
            var skuProducts = (await _productService.GetProducts(skus))
                .ToDictionary(p => p.Sku);
            foreach (var item in items)
            {
                if (skuProducts.TryGetValue(item.ProductSku, out var product))
                {
                    var contentItem = product.ContentItem;

                    foreach (var pricePart in contentItem.OfType<PricePart>()
                                 .Where(p => p.Price.Currency == _moneyService.CurrentDisplayCurrency))
                    {
                        item.Prices.Add(new PrioritizedPrice(0, pricePart.Price));
                    }
                }
            }
        }
    }
}
