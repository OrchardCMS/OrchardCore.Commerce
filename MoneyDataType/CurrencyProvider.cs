using System.Collections.Generic;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Money
{
    /// <summary>
    /// A simple currency provider that uses a static list of the most common predefined currencies
    /// </summary>
    public class CurrencyProvider : ICurrencyProvider
    {
        public CurrencyProvider()
        {
            KnownCurrencyTable.EnsureCurrencyTable();
        }

        public IEnumerable<ICurrency> Currencies 
            => KnownCurrencyTable.CurrencyTable.Values;

        public ICurrency GetCurrency(string isoSymbol) 
            => KnownCurrencyTable.CurrencyTable[isoSymbol];
    }
}
