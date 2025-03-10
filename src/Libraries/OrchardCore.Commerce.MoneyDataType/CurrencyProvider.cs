using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.MoneyDataType;

/// <summary>
/// A simple currency provider that uses a static list of the most common predefined currencies.
/// </summary>
public class CurrencyProvider : ICurrencyProvider
{
    public IEnumerable<ICurrency> Currencies =>
        KnownCurrencyTable.CurrencyTable.Values;

    static CurrencyProvider() => KnownCurrencyTable.EnsureCurrencyTable();

    public ICurrency GetCurrency(string isoCode)
    {
        if (isoCode is null) return Currency.UnspecifiedCurrency;

        return KnownCurrencyTable.CurrencyTable.TryGetValue(isoCode, out var value) ? value : null;
    }

    public bool IsKnownCurrency(string isoCode) => isoCode is not null && KnownCurrencyTable.CurrencyTable.ContainsKey(isoCode);
}
