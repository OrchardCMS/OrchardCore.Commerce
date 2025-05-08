using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using Xunit;
using static OrchardCore.Commerce.MoneyDataType.Currency;

namespace OrchardCore.Commerce.Tests;

public class CurrencyTests
{
    private static readonly IEnumerable<ICurrencyProvider> _currencyProviders =
    [
        new CurrencyProvider()
    ];

    public static TheoryData<string, decimal, string> TestData =>
        new()
        {
            { UsDollar.CurrencyIsoCode, 1234.56m, "$1,234.56" },
            { Euro.CurrencyIsoCode, 1234.56m, "€1234.56" },
            { JapaneseYen.CurrencyIsoCode, 1234.56m, "￥1,235" },
            { BritishPound.CurrencyIsoCode, 1234.56m, "£1,234.56" },
            { AustralianDollar.CurrencyIsoCode, 1234.56m, "$1,234.56" },
            { CanadianDollar.CurrencyIsoCode, 1234.56m, "$1,234.56" },
            { SwissFranc.CurrencyIsoCode, 1234.56m, "CHF 1’234.56" },
            { ChineseYuan.CurrencyIsoCode, 1234.56m, "¥1,234.56" },
        };

    [Theory]
    [MemberData(nameof(TestData))]
    public void KnownCurrenciesProperlyFormatAmounts(
        string currencyCode,
        decimal amount,
        string expectedFormat
    )
    {
        var currency = FromIsoCurrencyCode(currencyCode, _currencyProviders);
        Validate(currency, amount, expectedFormat);
    }

    [Fact]
    public void CustomCurrencyProperlyFormatsAmount()
    {
        var currency = new Currency("My FOO", "My FOO", "f", "FOO");
        Validate(
            currency,
            1234.56m,
            string.Create(CultureInfo.InvariantCulture, $"(FOO) {1234.56m:N}")
        );
    }

    private static void Validate(ICurrency currency, decimal amount, string expectedFormat)
    {
        var result = currency.ToString(amount).Replace(" ", string.Empty).Replace('￥', '¥');
        Assert.Equal(expectedFormat.Replace(" ", string.Empty).Replace('￥', '¥'), result);
    }
}
