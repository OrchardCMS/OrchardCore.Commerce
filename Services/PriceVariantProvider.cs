using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A simple price provider that obtains a price from a product by looking for a `PriceVariantsPart`
    /// </summary>
    public class PriceVariantProvider : IPriceProvider
    {
        private readonly IProductService _productService;
        private readonly IPredefinedValuesProductAttributeService _predefinedValuesService;

        public PriceVariantProvider(IProductService productService, IPredefinedValuesProductAttributeService predefinedValuesService)
        {
            _productService = productService;
            _predefinedValuesService = predefinedValuesService;
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
                            var attributesRestrictedToPredefinedValues = _predefinedValuesService
                                .GetProductAttributesRestrictedToPredefinedValues(product.ContentItem)
                                .Select(attr => attr.PartName + "." + attr.Name)
                                .ToHashSet();
                            var predefinedAttributes = item.Attributes
                                .Where(x => attributesRestrictedToPredefinedValues.Contains(x.AttributeName) && x is IPredefinedValuesProductAttributeValue ta)
                                .Cast<IPredefinedValuesProductAttributeValue>()
                                .OrderBy(x => x.AttributeName);
                            var variantKey = String.Join(
                                "-",
                                predefinedAttributes
                                    .Select(attr => attr.UntypedPredefinedValue)
                                    .Where(value => value != null));
                            if (priceVariantsPart.Variants.ContainsKey(variantKey))
                            {
                                item.Prices.Add(new PrioritizedPrice(1, priceVariantsPart.Variants[variantKey]));
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }
}
