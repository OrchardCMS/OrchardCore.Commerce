using System.Collections.Generic;
using Money;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Logic for selecting a price when multiple are available.
/// </summary>
public interface IPriceSelectionStrategy
{
    /// <summary>
    /// Returns a price from the provided collection of <paramref name="prices"/>.
    /// </summary>
    Amount SelectPrice(IEnumerable<PrioritizedPrice> prices);
}
