using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;
using Xunit;
using static OrchardCore.Commerce.Money.Currency;

namespace OrchardCore.Commerce.Tests
{
    public class MoneyServiceTests
    {
        [Fact]
        public void DefaultCurrencyWithoutSettingsOrProvidersIsDollar()
        {
            Assert.Equal("USD", new MoneyService(null, null).DefaultCurrency.IsoCode);
        }

        [Fact]
        public void DefaultCurrencyWithNullSettingsButNoProvidersIsDollar()
        {
            Assert.Equal(
                "USD",
                new MoneyService(null, new TestOptions<CommerceSettings>(
                    new CommerceSettings { })).DefaultCurrency.IsoCode);
        }

        [Fact]
        public void DefaultCurrencyWithSettingsSpecifyingDefaultCurrencyIsObserved()
        {
            Assert.Equal("EUR", _testMoneyService.DefaultCurrency.IsoCode);
        }

        [Fact]
        public void NotFoundDefaultCurrencyFallsBackToDollar()
        {
            Assert.Equal(
                "USD",
                new MoneyService(null, new TestOptions<CommerceSettings>(
                    new CommerceSettings {
                        DefaultCurrency = "WTF"
                    })).DefaultCurrency.IsoCode);
        }

        [Fact]
        public void CanEnumerateCurrenciesAcrossProviders()
        {
            Assert.Equal(new[] {
                Dollar, Euro, Yen, PoundSterling, AustralianDollar,
                CanadianDollar, SwissFranc, Renminbi, SwedishKrona,
                Currency.BitCoin, AnkhMorporkDollar, SixPence},
                _testMoneyService.Currencies);
        }

        [Fact]
        public void CanGetCurrenciesFromMultipleProviders()
        {
            Assert.Equal("EUR", _testMoneyService.GetCurrency("EUR").IsoCode);
            Assert.Equal("AMD", _testMoneyService.GetCurrency("AMD").IsoCode);
        }

        [Fact]
        public void UnknownCurrencyCodeGivesNullResult()
        {
            Assert.Null(_testMoneyService.GetCurrency("WTF"));
        }

        [Fact]
        public void CreateMakesAmountWithCurrency()
        {
            Amount amount = _testMoneyService.Create(42, "AMD");
            Assert.Equal(42, amount.Value);
            Assert.Equal(AnkhMorporkDollar, amount.Currency);
        }

        [Fact]
        public void EnsureCurrencyAddsRealCurrencyForCodeThatExists()
        {
            Amount amount = _testMoneyService.EnsureCurrency(new Amount(42, new Currency(null, null, "AMD", (CultureInfo)null)));
            Assert.Equal(42, amount.Value);
            Assert.Equal(AnkhMorporkDollar, amount.Currency);
        }

        private static readonly ICurrency AnkhMorporkDollar
            = new Currency("Ankh-Morpork Dollar", "$AM", "AMD", "dw-am");
        private static readonly ICurrency SixPence
            = new Currency("Sixpence", "6p", "SXP", "dw-am");
        private static readonly IMoneyService _testMoneyService = new MoneyService(
                    new ICurrencyProvider[] {
                        new CurrencyProvider(),
                        new AnkhMorporkCurrencyProvider()
                    },
                    new TestOptions<CommerceSettings>(new CommerceSettings
                    {
                        DefaultCurrency = "EUR"
                    }));

        private class AnkhMorporkCurrencyProvider : ICurrencyProvider
        {
            private ICurrency[] _currencies = new[] {
                AnkhMorporkDollar,
                SixPence
            };
            public IEnumerable<ICurrency> Currencies => _currencies;

            public ICurrency GetCurrency(string isoCode)
                => _currencies.FirstOrDefault(c => c.IsoCode == isoCode);
        }

        private class TestOptions<T> : IOptions<T> where T : class, new()
        {
            public TestOptions(T options) => Value = options;

            public T Value { get; }
        }
    }
}
