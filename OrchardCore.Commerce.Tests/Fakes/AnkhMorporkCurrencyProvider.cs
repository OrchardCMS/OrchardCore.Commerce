using System.Collections.Generic;
using System.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class AnkhMorporkCurrencyProvider : ICurrencyProvider
    {
        public static readonly ICurrency AnkhMorporkDollar
            = new Currency("Ankh-Morpork Dollar", "Ankh-Morpork Dollar", "$AM", "AMD");

        public static readonly ICurrency SixPence
            = new Currency("Sixpence", "Sixpence", "6p", "SXP");

        private ICurrency[] _currencies = new[] {
            AnkhMorporkDollar,
            SixPence
        };
        public IEnumerable<ICurrency> Currencies => _currencies;

        public ICurrency GetCurrency(string isoCode)
            => _currencies.FirstOrDefault(c => c.IsoCode == isoCode);
    }
}
