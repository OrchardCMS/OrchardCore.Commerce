using System.Collections.Generic;
using Money;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions;

public interface IPriceSelectionStrategy
{
    Amount SelectPrice(IEnumerable<PrioritizedPrice> prices);
}
