using System.Collections.Generic;
using System.Globalization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public class CurrencyTests
    {
        public static CurrencyTheoryData TestData
            => new CurrencyTheoryData {
                { Currency.Dollar, 1234.56m, "$1,234.56" },
                { Currency.Euro, 1234.56m, "1 234,56 €" },
                { Currency.Yen, 1234.56m, "¥1,235" },
                { Currency.PoundSterling, 1234.56m, "£1,234.56" },
                { Currency.AustralianDollar, 1234.56m, "$1,234.56" },
                { Currency.CanadianDollar, 1234.56m, "$1,234.56" },
                { Currency.SwissFranc, 1234.56m, "1’234.56 CHF" },
                { Currency.Renminbi, 1234.56m, "¥1,234.56" },
                { Currency.BitCoin, 1234.56789012m, "1,234.56789012 BTC" },
                { new Currency(null, null, "FOO", (CultureInfo)null), 1234.56m, "(FOO) 1,234.56" },
                { new Currency(null, null, null, (CultureInfo)null), 1234.56m, "(UNK) 1,234.56" }
            };

        [Theory]
        [MemberData(nameof(TestData))]
        public void CurrenciesProperlyFormatAmounts(ICurrency currency, decimal amount, string expectedFormat)
        {
            var result = currency.ToString(amount);
            Assert.Equal(expectedFormat, result);
        }

        public class CurrencyTheoryData : TheoryData<ICurrency, decimal, string> { }
    }
}
