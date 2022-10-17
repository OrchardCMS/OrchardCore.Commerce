using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OrchardCore.Commerce.MoneyDataType;

internal static class KnownCurrencyTable
{
    private static readonly object _lockObject = new();

    internal static IDictionary<string, ICurrency> CurrencyTable { get; private set; }

    internal static void EnsureCurrencyTable()
    {
        if (CurrencyTable == null) InitCurrencyCodeTable();
    }

    internal static ICurrency FromIsoCode(string isoCode)
    {
        EnsureCurrencyTable();
        return CurrencyTable[isoCode];
    }

    private static void InitCurrencyCodeTable()
    {
        static bool IsValid(CultureInfo cultureInfo) =>
            cultureInfo.Name.Contains('-') &&
            !cultureInfo.IsNeutralCulture &&
            !cultureInfo.EnglishName.StartsWith("Unknown Locale", StringComparison.Ordinal) &&
            !cultureInfo.EnglishName.StartsWith("Invariant Language", StringComparison.Ordinal);

        static int RankCultureByExpectedRelevance(CultureInfo cultureInfo)
        {
            var parts = cultureInfo.Name.Split('-');

            // Prioritize when the language and country ISO codes match, e.g. nl-NL, hu-HU, de-DE, etc.
            if (parts.Length == 2 && parts[0].ToUpperInvariant() == parts[1]) return 0;

            // English is usually a safe choice on the Internet.
            if (cultureInfo.TwoLetterISOLanguageName == "en") return 1;

            return int.MaxValue; // Fallback, the rest are sorted alphabetically.
        }

        lock (_lockObject)
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(IsValid).ToList();

            CurrencyTable = cultures
                .GroupBy(culture => culture.Name.Split('-').Last())
                .Select(group => group
                    .OrderBy(RankCultureByExpectedRelevance)
                    .ThenBy(culture => culture.EnglishName)
                    .First())
                .Select(culture => new Currency(culture))
                .Cast<ICurrency>()
                .Distinct(new CurrencyEqualityComparer())
                .ToDictionary(currency => currency.CurrencyIsoCode, currency => currency);

            CurrencyTable.Add("BTC", new Currency("BitCoin", "BitCoin", "₿", "BTC", 8));
            CurrencyTable.Add("---", Currency.UnspecifiedCurrency);
        }
    }

    private sealed class CurrencyEqualityComparer : IEqualityComparer<ICurrency>
    {
        public bool Equals(ICurrency left, ICurrency right) =>
            (left is null && right is null) ||
            StringComparer.OrdinalIgnoreCase.Equals(left?.CurrencyIsoCode, right?.CurrencyIsoCode);

        public int GetHashCode(ICurrency obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.CurrencyIsoCode);
    }
}
