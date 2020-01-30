using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Services
{
    public class PriceVariantsService : IPriceVariantsService
    {
        private readonly IPredefinedValuesProductAttributeService _predefinedValuesProductAttributeService;
        private readonly IMoneyService _moneyService;

        public PriceVariantsService(IPredefinedValuesProductAttributeService predefinedValuesProductAttributeService, IMoneyService moneyService)
        {
            _predefinedValuesProductAttributeService = predefinedValuesProductAttributeService;
            _moneyService = moneyService;
        }

        public Dictionary<string, Amount> GetPriceVariants(ContentItem product)
        {
            var part = product.As<PriceVariantsPart>();
            var keys = _predefinedValuesProductAttributeService.GetProductAttributesCombinations(product);

            return keys.Select(key =>
            {
                if (part?.Variants?.ContainsKey(key) == true) return part.Variants.FirstOrDefault(x => x.Key == key);
                else return new KeyValuePair<string, Amount>(key, new Amount(0, _moneyService.DefaultCurrency));
            }).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
