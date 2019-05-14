using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using static OrchardCore.Commerce.Money.Currency;

namespace OrchardCore.Commerce.Money
{
    /// <summary>
    /// A simple currency provider that uses a static list of the most common predefined currencies
    /// </summary>
    public class CurrencyProvider : ICurrencyProvider
    {
        private static readonly IEnumerable<ICurrency> _currencies = new List<ICurrency> {
            Dollar, Euro, Yen, PoundSterling, AustralianDollar,
            CanadianDollar, SwissFranc, Renminbi, SwedishKrona, Currency.BitCoin
        };
        private static readonly IDictionary<string, ICurrency> _currencyDictionary
            = _currencies.ToDictionary(c => c.IsoCode);

        public IEnumerable<ICurrency> Currencies => _currencies;

        public ICurrency GetCurrency(string isoSymbol) => _currencyDictionary[isoSymbol];

        public Task AddOrUpdateAsync(ICurrency currency)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(ICurrency currency)
        {
            return Task.CompletedTask;
        }
    }
}
