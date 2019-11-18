using System.Collections.Generic;
using System.Linq;
using Money;
using Money.Abstractions;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class AnkhMorporkCurrencyProvider : ICurrencyProvider
    {
        public static readonly ICurrency AnkhMorporkDollar
            = new Currency("Ankh-Morpork Dollar", "$AM", "AMD");

        public static readonly ICurrency SixPence
            = new Currency("Sixpence", "6p", "SXP");

        private readonly ICurrency[] _currencies = new[] {
            AnkhMorporkDollar,
            SixPence
        };
        public IEnumerable<ICurrency> Currencies => _currencies;

        public ICurrency GetCurrency(string isoCode)
            => _currencies.FirstOrDefault(c => c.CurrencyIsoCode == isoCode);

        public bool IsKnownCurrency(string isoCode) => _currencies.Any(c => string.Equals(c.CurrencyIsoCode, isoCode, System.StringComparison.InvariantCultureIgnoreCase));
    }
}
