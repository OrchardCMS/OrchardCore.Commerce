using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Tests.Fakes;

public class AnkhMorporkCurrencyProvider : ICurrencyProvider
{
    public static readonly ICurrency AnkhMorporkDollar
        = new Currency("Ankh-Morpork Dollar", "Ankh-Morpork Dollar", "$AM", "AMD");

    public static readonly ICurrency SixPence
        = new Currency("Sixpence", "Sixpence", "6p", "SXP");

    private readonly ICurrency[] _currencies =
    {
        AnkhMorporkDollar,
        SixPence,
    };

    public IEnumerable<ICurrency> Currencies => _currencies;

    public ICurrency GetCurrency(string isoCode) =>
        _currencies.Find(currency => currency.CurrencyIsoCode == isoCode);

    public bool IsKnownCurrency(string isoCode) =>
        _currencies.Exists(currency => string.Equals(currency.CurrencyIsoCode, isoCode, StringComparison.OrdinalIgnoreCase));
}
