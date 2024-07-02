using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;
using System.Text.Json;
using Xunit;
using static OrchardCore.Commerce.MoneyDataType.Currency;

namespace OrchardCore.Commerce.Tests;

public class AmountTests
{
    [Fact]
    public void CantConstructAnAmountWithNullCurrency() =>
        Assert.Equal(new Amount(1, (ICurrency)null), new Amount(1, UnspecifiedCurrency));

    [Fact]
    public void AmountsCanBeAdded() =>
        Assert.Equal(new Amount(42.23M, Euro), new Amount(21.12M, Euro) + new Amount(21.11M, Euro));

    [Fact]
    public void AddingDifferentCurrenciesThrows() =>
        Assert.Throws<InvalidOperationException>(() => new Amount(1, UsDollar) + new Amount(1, Euro));

    [Fact]
    public void AmountsCanBeSubtracted() =>
        Assert.Equal(new Amount(0.01M, Euro), new Amount(21.12M, Euro) - new Amount(21.11M, Euro));

    [Fact]
    public void SubtractingDifferentCurrenciesThrows() =>
        Assert.Throws<InvalidOperationException>(() => new Amount(1, UsDollar) - new Amount(1, Euro));

    [Fact]
    public void AmountsCanBeNegated()
    {
        Assert.Equal(new Amount(-42, Euro), -new Amount(42, Euro));
        Assert.Equal(new Amount(42, Euro), -new Amount(-42, Euro));
    }

    [Fact]
    public void OneCanMultiplyAnAmountByANumber() // #spell-check-ignore-line
    {
        Assert.Equal(new Amount(42.34M, Euro), 2 * new Amount(21.17M, Euro));
        Assert.Equal(new Amount(42.34M, Euro), new Amount(21.17M, Euro) * 2);
        Assert.Equal(new Amount(42.34M, Euro), 2f * new Amount(21.17M, Euro));
        Assert.Equal(new Amount(42.34M, Euro), new Amount(21.17M, Euro) * 2f);
        Assert.Equal(new Amount(42.34M, Euro), 2d * new Amount(21.17M, Euro));
        Assert.Equal(new Amount(42.34M, Euro), new Amount(21.17M, Euro) * 2d);
        Assert.Equal(new Amount(42.34M, Euro), 2m * new Amount(21.17M, Euro));
        Assert.Equal(new Amount(42.34M, Euro), new Amount(21.17M, Euro) * 2m);
    }

    [Fact]
    public void AmountsOfTheSameCurrencyCanBeCompared()
    {
        Assert.Equal(0, new Amount(1, Euro).CompareTo(new Amount(1, Euro)));
        Assert.Equal(1, new Amount(2, Euro).CompareTo(new Amount(1, Euro)));
        Assert.Equal(-1, new Amount(1, Euro).CompareTo(new Amount(2, Euro)));
        Assert.True(new Amount(1, Euro) == new Amount(1, Euro));
        Assert.False(new Amount(1, Euro) == new Amount(2, Euro));
        Assert.True(new Amount(1, Euro) != new Amount(2, Euro));
        Assert.False(new Amount(1, Euro) != new Amount(1, Euro));
        Assert.True(new Amount(1, Euro) < new Amount(2, Euro));
        Assert.False(new Amount(1, Euro) < new Amount(1, Euro));
        Assert.False(new Amount(2, Euro) < new Amount(1, Euro));
        Assert.True(new Amount(2, Euro) > new Amount(1, Euro));
        Assert.False(new Amount(1, Euro) > new Amount(1, Euro));
        Assert.False(new Amount(1, Euro) > new Amount(2, Euro));
        Assert.True(new Amount(1, Euro) <= new Amount(2, Euro));
        Assert.True(new Amount(1, Euro) <= new Amount(1, Euro));
        Assert.False(new Amount(2, Euro) <= new Amount(1, Euro));
        Assert.True(new Amount(2, Euro) >= new Amount(1, Euro));
        Assert.True(new Amount(1, Euro) >= new Amount(1, Euro));
        Assert.False(new Amount(1, Euro) >= new Amount(2, Euro));
    }

    [Fact]
    public void ComparingAmountsOfDifferentCurrenciesThrows()
    {
        Assert.Throws<InvalidOperationException>(() => new Amount(1, Euro).CompareTo(new Amount(1, UsDollar)));
        Assert.Throws<InvalidOperationException>(() => new Amount(1, Euro) < new Amount(2, UsDollar));
        Assert.Throws<InvalidOperationException>(() => new Amount(2, Euro) > new Amount(1, UsDollar));
        Assert.Throws<InvalidOperationException>(() => new Amount(1, Euro) <= new Amount(2, UsDollar));
        Assert.Throws<InvalidOperationException>(() => new Amount(2, Euro) >= new Amount(1, UsDollar));
    }

    [Fact]
    public void SerialisationPersistsValidValues()
    {
        var originalAmount = new Amount(1.23M, Euro);

        var json = JsonSerializer.Serialize(originalAmount);
        var deserializedAmount = JsonSerializer.Deserialize<Amount>(json);

        Assert.Equal(originalAmount, deserializedAmount);
    }

    [Fact]
    public void SerialisationPersistsUnknownCurrencies()
    {
        var unknown = new Currency("My FOO", "My FOO", "f", "FOO");

        var originalAmount = new Amount(1.23M, unknown);

        var json = JsonSerializer.Serialize(originalAmount);
        var deserializedAmount = JsonSerializer.Deserialize<Amount>(json);

        Assert.True(originalAmount == deserializedAmount);

        Assert.Equal(originalAmount, deserializedAmount);
    }
}
