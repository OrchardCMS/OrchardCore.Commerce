using System.Collections.Generic;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPriceSelectionStrategy
    {
        // TODO: include attributes in price strategy
        Amount SelectPrice(IList<Amount> prices);
    }
}
