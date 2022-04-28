using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A price selection strategy that selects the lowest price of the highest priority prices.
///
/// This price selection strategy will fail if the list of amounts
/// isn't homogeneous in currency, so calling code is responsible for filtering
/// for a specific currency before calling.
/// </summary>
public class SimplePriceStrategy : IPriceSelectionStrategy
{
    public Amount SelectPrice(IEnumerable<PrioritizedPrice> prices)
    {
        var priceCollection = prices as ICollection<PrioritizedPrice> ?? prices?.ToList();
        if (priceCollection?.Any() != true) return new Amount();

        var maxPriority = priceCollection.Max(price => price.Priority);
        return priceCollection
            .Where(pp => pp.Priority == maxPriority)
            .Min(pp => pp.Price);
    }
}
