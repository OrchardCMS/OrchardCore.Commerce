using System.Collections.Generic;
using System.Linq;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// A price selection strategy that selects the lowest price of the highest priority prices.
    /// 
    /// This price selection strategy will fail if the list of amounts
    /// isn't homogeneous in currency, so calling code is responsible for filtering
    /// for a specific currency before calling.
    /// </summary>
    public class SimplePriceStrategy : IPriceSelectionStrategy
    {
        public Amount SelectPrice(IList<PrioritizedPrice> prices)
            => prices is null
                || !prices.Any()
                    ? new Amount()
                    : prices
                        .Where(pp => pp.Priority == prices.Max(pp => pp.Priority))
                        .Min(pp => pp.Price);
    }
}
