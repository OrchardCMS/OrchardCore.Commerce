using System.Collections.Generic;
using System.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Services
{
    public class LowestPriceStrategy : IPriceSelectionStrategy
    {
        public Amount SelectPrice(IList<Amount> prices) => prices.Min();
    }
}
