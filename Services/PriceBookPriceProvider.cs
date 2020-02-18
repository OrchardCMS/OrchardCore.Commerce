using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A price provider that obtains a price based on price book rules
    /// </summary>
    public class PriceBookPriceProvider : IPriceProvider
    {
        private readonly IProductService _productService;
        private readonly IPriceBookService _priceBookService;

        public PriceBookPriceProvider(IProductService productService, IPriceBookService priceBookService)
        {
            _productService = productService;
            _priceBookService = priceBookService;
        }

        public int Order => 0; // Keep the same level as the native price book provider so price variants can override

        public async Task AddPrices(IList<ShoppingCartItem> items)
        {
            var priceBooks = await _priceBookService.GetPriceBooksFromRules();
            if (priceBooks.Any())
            {
                var skus = items.Select(item => item.ProductSku).Distinct().ToArray();
                var skuProducts = (await _productService.GetProducts(skus))
                    .ToDictionary(p => p.Sku);
                foreach (var item in items)
                {
                    if (skuProducts.TryGetValue(item.ProductSku, out var product))
                    {
                        var priceBookPrices = await _priceBookService.GetPriceBookPrices(priceBooks, product);
                        foreach (var priceBookPrice in priceBookPrices)
                        {
                            item.Prices.Add(new PrioritizedPrice(0, priceBookPrice.Price));
                        }
                    }
                }
            }
        }
    }
}
