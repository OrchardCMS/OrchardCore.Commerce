using System.Collections.Generic;
using System.Linq;
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

            return keys
                .Where(key => part?.Variants?.ContainsKey(key) == true)
                .ToDictionary(key => key, key => part.Variants[key]);
        }
    }
}
