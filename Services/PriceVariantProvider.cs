using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public PriceVariantProvider(IProductService productService)
        {
            _productService = productService;
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
                            var tpavs = item.Attributes.Where(x => x is TextProductAttributeValue ta).Cast<TextProductAttributeValue>();
                            var variantKey = string.Join("-", tpavs.Select(x => x.Value.FirstOrDefault()).Where(x => x != null));
                            if (priceVariantsPart.Variants.ContainsKey(variantKey))
                            {
                                item.Prices.Add(new Money.Amount(priceVariantsPart.Variants[variantKey], priceVariantsPart.BasePrice.Currency));
                                continue;
                            }
                        }

                        item.Prices.Add(priceVariantsPart.BasePrice);
                    }
                }
            }
        }
    }
}
