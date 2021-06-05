using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class FakePriceService : IPriceService
    {
        public Task<IEnumerable<ShoppingCartItem>> AddPrices(IEnumerable<ShoppingCartItem> items)
            => Task.FromResult(
                items.Select(
                    (item, i) => item.WithPrice(new PrioritizedPrice(0, new Amount(42 + i++, Currency.USDollar)))));
    }
}
