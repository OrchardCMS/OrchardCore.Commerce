using System.Collections.Generic;
using System.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A price selection strategy that selects the lowest price.
    /// 
    /// This price selection strategy will fail if the list of amounts
    /// isn't homogeneous in currency, so calling code is responsible for filtering
    /// for a specific currency before calling.
    /// </summary>
    public class LowestPriceStrategy : IPriceSelectionStrategy
    {
        public Amount SelectPrice(IList<Amount> prices)
            => prices is null || !prices.Any() ? new Amount() : prices.Min();
    }
}
