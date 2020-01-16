using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A price service that asks all available price providers to add prices to a list of shopping cart items
    /// </summary>
    public class PriceService : IPriceService
    {
        private IEnumerable<IPriceProvider> _providers;

        public PriceService(IEnumerable<IPriceProvider> priceProviders)
        {
            _providers = priceProviders;
        }

        public async Task AddPrices(IList<ShoppingCartItem> items)
        {
            foreach (var priceProvider in _providers.OrderBy(p => p.Order))
            {
                await priceProvider.AddPrices(items);
            }
        }

        public Dictionary<string, decimal> BuildVariantPrices(IEnumerable<string> keys, PricePart part)
        {
            return keys.Select(key =>
            {
                if (part.VariantsPrices != null && part.VariantsPrices.ContainsKey(key)) return part.VariantsPrices.FirstOrDefault(x => x.Key == key);
                else return new KeyValuePair<string, decimal>(key, part.Price.Value);
            }).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
