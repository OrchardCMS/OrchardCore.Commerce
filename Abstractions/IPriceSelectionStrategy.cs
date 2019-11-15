using System.Collections.Generic;
using Money;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPriceSelectionStrategy
    {
        // TODO: include attributes in price strategy
        Amount SelectPrice(IList<Amount> prices);
    }
}
