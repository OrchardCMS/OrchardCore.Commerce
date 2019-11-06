using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public class CurrencyTests
    {
        public static CurrencyTheoryData TestData
            => new CurrencyTheoryData {
                { Currency.USDollar, 1234.56m, "$1,234.56" },
                { Currency.Euro, 1234.56m, "1 234,56 €" },
                { Currency.JapaneseYen, 1234.56m, "¥1,235" },
                { Currency.BritishPound, 1234.56m, "£1,234.56" },
                { Currency.AustralianDollar, 1234.56m, "$1,234.56" },
                { Currency.CanadianDollar, 1234.56m, "$1,234.56" },
                { Currency.SwissFranc, 1234.56m, "CHF 1’234.56" },
                { Currency.ChineseYuan, 1234.56m, "¥1,234.56" },
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
