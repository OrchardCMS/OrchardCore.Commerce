using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class FakePriceService : IPriceService
    {
        public Task AddPrices(IList<ShoppingCartItem> items)
        {
            var i = 0;
            foreach (ShoppingCartItem item in items)
            {
                item.Prices.Add(new Amount(42 + i++, Currency.USDollar));
            }
            return Task.CompletedTask;
        }
    }
}
