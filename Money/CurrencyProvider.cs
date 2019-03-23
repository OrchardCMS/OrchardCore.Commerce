using System.Collections.Generic;
using System.Linq;
using OrchardCore.Commerce.Abstractions;
using static OrchardCore.Commerce.Money.Currency;

namespace OrchardCore.Commerce.Money
{
    public class CurrencyProvider : ICurrencyProvider
    {
        private static readonly IEnumerable<ICurrency> _currencies = new List<ICurrency> {
            Dollar, Euro, Yen, PoundSterling, AustralianDollar,
            CanadianDollar, SwissFranc, Renminbi, Currency.BitCoin
        };
        private static readonly IDictionary<string, ICurrency> _currencyDictionary
            = _currencies.ToDictionary(c => c.IsoCode);

        public IEnumerable<ICurrency> Currencies => _currencies;

        public ICurrency GetCurrency(string isoSymbol) => _currencyDictionary[isoSymbol];
    }
}
