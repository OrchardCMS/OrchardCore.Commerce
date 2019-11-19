using Money;
using Money.Abstractions;
using Xunit;

using static Money.Currency;

namespace OrchardCore.Commerce.Tests
{
    public class CurrencyTests
    {
        public static CurrencyTheoryData TestData
            => new CurrencyTheoryData {
                { USDollar, 1234.56m, "$1,234.56" },
                { Euro, 1234.56m, "1 234,56 €" },
                { JapaneseYen, 1234.56m, "¥1,235" },
                { BritishPound, 1234.56m, "£1,234.56" },
                { AustralianDollar, 1234.56m, "$1,234.56" },
                { CanadianDollar, 1234.56m, "$1,234.56" },
                { SwissFranc, 1234.56m, "CHF 1’234.56" },
                { ChineseYuan, 1234.56m, "¥1,234.56" },
                { new Currency("My FOO", "f", "FOO"), 1234.56m, "(FOO) 1,234.56" }
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
