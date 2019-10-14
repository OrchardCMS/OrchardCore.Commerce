using System.Collections.Generic;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Tests
{
    public class FakePriceService : IPriceService
    {
        public void AddPrices(IList<ShoppingCartItem> items)
        {
            int i = 0;
            foreach(ShoppingCartItem item in items)
            {
                item.Prices.Add(new Amount(42 + i++, Currency.Dollar));
            }
        }
    }
}
