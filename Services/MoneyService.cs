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
        private ICurrencyProvider _currencyProvider;
        //private IEnumerable<ICurrencyProvider> _currencyProviders;
        private readonly CommerceSettings _options;

        public MoneyService(
            ICurrencyProvider currencyProvider,
            //IEnumerable<ICurrencyProvider> currencyProviders,
            IOptions<CommerceSettings> options)
        {
            _currencyProvider = currencyProvider;
            _options = options.Value;
        }

        public IEnumerable<ICurrency> Currencies => _currencyProvider.Currencies;   // (p => p.Currencies);

        public ICurrency DefaultCurrency => GetCurrency(_options.DefaultCurrency ?? "USD");

        public Amount Create(decimal value, string currencyIsoSymbol) => new Amount(value, GetCurrency(currencyIsoSymbol));

        public Amount EnsureCurrency(Amount amount)
            => amount.Currency != null && amount.Currency.IsResolved
            ? amount
            : new Amount(amount.Value, GetCurrency(amount.Currency.IsoCode));

        public ICurrency GetCurrency(string isoSymbol)
            => Currencies.FirstOrDefault(p => p.IsoCode.Equals(isoSymbol, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new ArgumentOutOfRangeException(nameof(isoSymbol), isoSymbol, "Currency not found.");
    }
}