using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Settings;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// Exposes the data from all enabled currency providers under a simpler, unified interface.
/// </summary>
public class MoneyService : IMoneyService
{
    private readonly IEnumerable<ICurrencyProvider> _currencyProviders;
    private readonly CurrencySettings _options;
    private readonly IEnumerable<ICurrencySelector> _currencySelectors;

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

    public ICurrency CurrentDisplayCurrency => _currencySelectors.First().CurrentDisplayCurrency ?? DefaultCurrency;

    public MoneyService(
        IEnumerable<ICurrencyProvider> currencyProviders,
        IOptions<CurrencySettings> options,
        IEnumerable<ICurrencySelector> currencySelectors)
    {
        _currencyProviders = currencyProviders ?? [];
        _options = options?.Value;
        _currencySelectors = currencySelectors ?? [];
    }

    public Amount Create(decimal value, string currencyIsoCode = null) =>
        new(value, currencyIsoCode == null ? DefaultCurrency : GetCurrency(currencyIsoCode));

    public Amount EnsureCurrency(Amount amount) =>
        new(amount.Value, GetCurrency(amount.Currency.CurrencyIsoCode));

    public ICurrency GetCurrency(string currencyIsoCode) =>
        Currency.FromIsoCode(currencyIsoCode, _currencyProviders);
}
