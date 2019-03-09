using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions
{
    public interface ICurrencyProvider
    {
        IEnumerable<ICurrency> Currencies { get; }
        ICurrency GetCurrency(string isoSymbol);
    }
}
