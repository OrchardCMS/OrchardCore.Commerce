using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// Exposes the data from all enabled currency providers under a simpler, unified interface.
/// </summary>
public class MoneyService : IMoneyService
{
    private readonly IEnumerable<ICurrencyProvider> _currencyProviders;
    private readonly CommerceSettings _options;
    private readonly ICurrencySelector _currencySelector;

    public IEnumerable<ICurrency> Currencies =>
        _currencyProviders
            .SelectMany(provider => provider.Currencies)
            .OrderBy(currency => currency.CurrencyIsoCode);

    public ICurrency DefaultCurrency
    {
        get
        {
            var defaultIsoCode = _options?.DefaultCurrency;
            return string.IsNullOrEmpty(defaultIsoCode)
                ? Currency.UsDollar
                : GetCurrency(defaultIsoCode) ?? Currency.UsDollar;
        }
    }

    public ICurrency CurrentDisplayCurrency => _currencySelector.CurrentDisplayCurrency ?? DefaultCurrency;

    public MoneyService(
        IEnumerable<ICurrencyProvider> currencyProviders,
        IOptions<CommerceSettings> options,
        ICurrencySelector currencySelector)
    {
        _currencyProviders = currencyProviders ?? Array.Empty<ICurrencyProvider>();
        _options = options?.Value;
        _currencySelector = currencySelector;
    }

    public Amount Create(decimal value, string currencyIsoCode) =>
        new(value, GetCurrency(currencyIsoCode));

    public Amount EnsureCurrency(Amount amount) =>
        new(amount.Value, GetCurrency(amount.Currency.CurrencyIsoCode));

    public ICurrency GetCurrency(string currencyIsoCode) =>
        Currency.FromIsoCode(currencyIsoCode, _currencyProviders);
}
