using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A simple price provider that obtains a price from a product by looking for a `PricePart`
    /// </summary>
    public class PriceProvider : IPriceProvider
    {
        private readonly IProductService _productService;

        public PriceProvider(IProductService productService)
        {
            _productService = productService;
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
                    var pricePart = product.ContentItem.As<PricePart>();
                    if (pricePart is object)
                    {
                        item.Prices.Add(new ProductPrice(pricePart.Price));
                    }
                }
            }
        }
    }
}
