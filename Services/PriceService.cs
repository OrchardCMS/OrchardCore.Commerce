using System.Collections.Generic;
using System.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Services
{
    public class PriceService : IPriceService
    {
        private IEnumerable<IPriceProvider> _providers;

        public PriceService(IEnumerable<IPriceProvider> priceProviders)
        {
            _providers = priceProviders;
        }

        public void AddPrices(IList<ShoppingCartItem> items)
        {
            foreach (var priceProvider in _providers.OrderBy(p => p.Order))
            {
                priceProvider.AddPrices(items);
            }
        }
    }
}
