using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.Tests.Fakes;
using Xunit;

namespace OrchardCore.Commerce.Tests;

public class MoneyServiceTests
{
    [Fact]
    public void DefaultCurrencyWithoutSettingsOrProvidersIsDollar() =>
        Assert.Equal(
            "USD",
            new MoneyService(currencyProviders: null, options: null, currencySelectors: [])
                .DefaultCurrency
                .CurrencyIsoCode);

    [Fact]
    public void DefaultCurrencyWithNullSettingsButNoProvidersIsDollar() =>
        Assert.Equal(
            "USD",
            new MoneyService(
                    currencyProviders: null,
                    new TestOptions<CurrencySettings>(new CurrencySettings()),
                    currencySelectors: [])
                .DefaultCurrency
                .CurrencyIsoCode);

    [Fact]
    public void DefaultCurrencyWithSettingsSpecifyingDefaultCurrencyIsObserved() =>
        Assert.Equal("EUR", new TestMoneyService().DefaultCurrency.CurrencyIsoCode);

    [Fact]
    public void NotFoundDefaultCurrencyFallsBackToDollar() =>
        Assert.Equal(
            "USD",
            new MoneyService(
                    currencyProviders: null,
                    new TestOptions<CurrencySettings>(new CurrencySettings { DefaultCurrency = "WTF" }),
                    currencySelectors: [])
                .DefaultCurrency
                .CurrencyIsoCode);

    [Fact]
    public void CanGetCurrenciesFromMultipleProviders()
    {
        Assert.Equal("EUR", new TestMoneyService().GetCurrency("EUR").CurrencyIsoCode);
        Assert.Equal("AMD", new TestMoneyService().GetCurrency("AMD").CurrencyIsoCode);
    }

    [Fact]
    public void UnknownCurrencyCodeGivesNullResult() => Assert.Null(new TestMoneyService().GetCurrency("WTF"));

    [Fact]
    public void CreateMakesAmountWithCurrency()
    {
        var service = new TestMoneyService();
        var amount = service.Create(42, "AMD");

        Assert.Equal(42, amount.Value);
        Assert.Equal(service.GetCurrency("AMD"), amount.Currency);
    }

    [Fact]
    public void EnsureCurrencyAddsRealCurrencyForCodeThatExists()
    {
        var service = new TestMoneyService();
        var amount = service.EnsureCurrency(new Amount(42, new Currency("My Fake", "My Fake", "X", "AMD")));
        Assert.Equal(42, amount.Value);
        Assert.Equal(service.GetCurrency("AMD"), amount.Currency);
    }
}
