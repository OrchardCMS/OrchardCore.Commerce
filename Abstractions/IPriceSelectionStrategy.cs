using System.Collections.Generic;
using Money;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPriceSelectionStrategy
    {
        // TODO: include attributes in price strategy
        Amount SelectPrice(IList<PrioritizedPrice> prices);
    }
}
