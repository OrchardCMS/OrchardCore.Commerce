using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A simple price provider that obtains a price from a product by looking for a `PriceVariantsPart`
    /// </summary>
    public class PriceVariantProvider : IPriceProvider
    {
        private readonly IProductService _productService;
        private readonly IMoneyService _moneyService;

        public PriceVariantProvider(IProductService productService, IMoneyService moneyService)
        {
            _productService = productService;
            _moneyService = moneyService;
        }

        public int Order => 1;

        public async Task AddPrices(IList<ShoppingCartItem> items)
        {
            var skus = items.Select(item => item.ProductSku).Distinct().ToArray();
            var skuProducts = (await _productService.GetProducts(skus))
                .ToDictionary(p => p.Sku);
            foreach (var item in items)
            {
                if (skuProducts.TryGetValue(item.ProductSku, out var product))
                {
                    var priceVariantsPart = product.ContentItem.As<PriceVariantsPart>();
                    if (priceVariantsPart != null)
                    {
                        if (priceVariantsPart.Variants != null)
                        {
                            var tpavs = item.Attributes.Where(x => x is IPredefinedValuesProductAttributeValue ta).Cast<IPredefinedValuesProductAttributeValue>();
                            var variantKey = string.Join("-", tpavs.Select(x => x.UntypedPredefinedValue).Where(x => x != null));
                            if (priceVariantsPart.Variants.ContainsKey(variantKey))
                            {
                                item.Prices.Add(priceVariantsPart.Variants[variantKey]);
                                continue;
                            }
                        }

                        item.Prices.Add(new Amount(0, _moneyService.DefaultCurrency));
                    }
                }
            }
        }
    }
}
