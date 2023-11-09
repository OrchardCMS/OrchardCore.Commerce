using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;

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
