using System.Linq;
using Money;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.Tests.Fakes;
using Xunit;

namespace OrchardCore.Commerce.Tests
{
    public partial class MoneyServiceTests
    {
        [Fact]
        public void DefaultCurrencyWithoutSettingsOrProvidersIsDollar()
        {
            Assert.Equal("USD", new MoneyService(null, null).DefaultCurrency.CurrencyIsoCode);
        }

        [Fact]
        public void DefaultCurrencyWithNullSettingsButNoProvidersIsDollar()
        {
            Assert.Equal(
                "USD",
                new MoneyService(
                    null,
                    new TestOptions<CommerceSettings>(new CommerceSettings { })
                    ).DefaultCurrency.CurrencyIsoCode);
        }

        [Fact]
        public void DefaultCurrencyWithSettingsSpecifyingDefaultCurrencyIsObserved()
        {
            Assert.Equal("EUR", new TestMoneyService().DefaultCurrency.CurrencyIsoCode);
        }

        [Fact]
        public void NotFoundDefaultCurrencyFallsBackToDollar()
        {
            Assert.Equal(
                "USD",
                new MoneyService(null, new TestOptions<CommerceSettings>(
                    new CommerceSettings
                    {
                        DefaultCurrency = "WTF"
                    })).DefaultCurrency.CurrencyIsoCode);
        }

        [Fact]
        public void EnsureCurrenciesAcrossAllProviders()
        {
            Assert.Equal(114, new TestMoneyService().Currencies.Count());
        }

        [Fact]
        public void CanGetCurrenciesFromMultipleProviders()
        {
            Assert.Equal("EUR", new TestMoneyService().GetCurrency("EUR").CurrencyIsoCode);
            Assert.Equal("AMD", new TestMoneyService().GetCurrency("AMD").CurrencyIsoCode);
        }

        [Fact]
        public void UnknownCurrencyCodeGivesNullResult()
        {
            Assert.Null(new TestMoneyService().GetCurrency("WTF"));
        }

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
            var amount = service.EnsureCurrency(new Amount(42, new Currency("My Fake", "X", "AMD")));
            Assert.Equal(42, amount.Value);
            Assert.Equal(service.GetCurrency("AMD"), amount.Currency);
        }
    }
}
