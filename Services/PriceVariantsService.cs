using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Services
{
    public class PriceVariantsService : IPriceVariantsService
    {
        private readonly IPredefinedValuesProductAttributeService _predefinedValuesProductAttributeService;

        public PriceVariantsService(IPredefinedValuesProductAttributeService predefinedValuesProductAttributeService)
        {
            _predefinedValuesProductAttributeService = predefinedValuesProductAttributeService;
        }

        public Dictionary<string, decimal> GetPriceVariants(ContentItem product)
        {
            var part = product.As<PriceVariantsPart>();
            var basePrice = part?.BasePrice.Value ?? 0;
            var keys = _predefinedValuesProductAttributeService.GetProductAttributesCombinations(product);

            return keys.Select(key =>
            {
                if (part?.Variants?.ContainsKey(key) == true) return part.Variants.FirstOrDefault(x => x.Key == key);
                else return new KeyValuePair<string, decimal>(key, basePrice);
            }).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
