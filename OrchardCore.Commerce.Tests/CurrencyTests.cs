using Money;
using Money.Abstractions;
using System;
using Xunit;
using static Money.Currency;

namespace OrchardCore.Commerce.Tests;

public class CurrencyTests
{
    public static CurrencyTheoryData TestData
        => new()
        {
            { UsDollar, 1234.56m, "$1,234.56" },
            { Euro, 1234.56m, "1.234,56 €" },
            { JapaneseYen, 1234.56m, "￥1,235" },
            { BritishPound, 1234.56m, "£1,234.56" },
            { AustralianDollar, 1234.56m, "$1,234.56" },
            { CanadianDollar, 1234.56m, "$1,234.56" },
            { SwissFranc, 1234.56m, "CHF 1’234.56" },
            { ChineseYuan, 1234.56m, "¥1,234.56" },
            { new Currency("My FOO", "My FOO", "f", "FOO"), 1234.56m, FormattableString.Invariant($"(FOO) {1234.56m:N}") },
        };

    [Theory]
    [MemberData(nameof(TestData))]
    public void CurrenciesProperlyFormatAmounts(ICurrency currency, decimal amount, string expectedFormat)
    {
        var result = currency.ToString(amount).Replace(" ", string.Empty).Replace("￥", "¥");
        Assert.Equal(expectedFormat.Replace(" ", string.Empty).Replace("￥", "¥"), result);
    }

    public class CurrencyTheoryData : TheoryData<ICurrency, decimal, string> { }
}
