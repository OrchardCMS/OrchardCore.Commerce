using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.Services
{
    /// <summary>
    /// Exposes the data from all enabled currency providers under a simpler, unified interface.
    /// </summary>
    public class MoneyService : IMoneyService
    {
        private IEnumerable<ICurrencyProvider> _currencyProviders;
        private readonly CommerceSettings _options;

        public MoneyService(
            IEnumerable<ICurrencyProvider> currencyProviders, 
            IOptions<CommerceSettings> options)
        {
            _currencyProviders = currencyProviders ?? Array.Empty<ICurrencyProvider>();
            _options = options?.Value;
        }

        public IEnumerable<ICurrency> Currencies
            => _currencyProviders.SelectMany(p => p.Currencies);

        public ICurrency DefaultCurrency
        {
            get
            {
                var defaultIsoCode = _options?.DefaultCurrency;
                return string.IsNullOrEmpty(defaultIsoCode) 
                    ? Currency.USDollar 
                    : GetCurrency(_options.DefaultCurrency) 
                    ?? Currency.USDollar;
            }
        }

        public Amount Create(decimal value, string currencyIsoCode)
            => new Amount(value, GetCurrency(currencyIsoCode));

        public Amount EnsureCurrency(Amount amount)
            => new Amount(amount.Value, GetCurrency(amount.Currency.CurrencyIsoCode));

        public ICurrency GetCurrency(string currencyIsoCode)
            => Currency.FromISOCode(currencyIsoCode, _currencyProviders); 
    }
}