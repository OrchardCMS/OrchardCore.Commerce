using Money.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Money;

internal static class KnownCurrencyTable
{
    private static readonly object _obj = new();

    internal static IDictionary<string, ICurrency> CurrencyTable { get; private set; }

    internal static void EnsureCurrencyTable()
    {
        if (CurrencyTable == null) InitCurrencyCodeTable();
    }

    private sealed class CurrencyEqualityComparer : IEqualityComparer<ICurrency>
    {
        public bool Equals(ICurrency left, ICurrency right) =>
            (left is null && right is null) ||
            left?.CurrencyIsoCode?.Equals(right?.CurrencyIsoCode, StringComparison.OrdinalIgnoreCase) == true;
        public int GetHashCode(ICurrency obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.CurrencyIsoCode);
    }

    private static void InitCurrencyCodeTable()
    {
        static bool Valid(CultureInfo cultureInfo) =>
            !cultureInfo.IsNeutralCulture &&
            !cultureInfo.EnglishName.StartsWith("Unknown Locale", StringComparison.Ordinal) &&
            !cultureInfo.EnglishName.StartsWith("Invariant Language", StringComparison.Ordinal);

        lock (_obj)
        {
            CurrencyTable = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(Valid)
                .Select(c => new Currency(c)).Cast<ICurrency>()
                .Distinct(new CurrencyEqualityComparer())
                .ToDictionary(k => k.CurrencyIsoCode, e => e);

            CurrencyTable.Add("BTC", new Currency("BitCoin", "BitCoin", "₿", "BTC", 8));
            CurrencyTable.Add("---", new Currency("Unspecified", "Unspecified", "---", "---"));
        }
    }

    internal static ICurrency FromIsoCode(string isoCode)
    {
        EnsureCurrencyTable();
        return CurrencyTable[isoCode];
    }
}
