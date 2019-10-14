using System.Collections.Generic;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPriceSelectionStrategy
    {
        Amount SelectPrice(IList<Amount> prices);
    }
}
