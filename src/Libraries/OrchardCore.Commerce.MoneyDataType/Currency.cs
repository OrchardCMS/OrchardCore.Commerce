using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Serialization;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.MoneyDataType;

[JsonConverter(typeof(CurrencyConverter))]
[Newtonsoft.Json.JsonConverter(typeof(LegacyCurrencyConverter))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly partial struct Currency : ICurrency, IEquatable<Currency>
{
    public const int DefaultDecimalDigits = 2;

    public int DecimalPlaces { get; }

    public string CurrencyIsoCode { get; }

    public string NativeName { get; }

    public string EnglishName { get; }

    public string Symbol { get; }

    public CultureInfo Culture { get; }

    private string DebuggerDisplay => CurrencyIsoCode;

    public Currency(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        if (culture.EnglishName.StartsWith("Unknown Locale", StringComparison.Ordinal) ||
            culture.EnglishName.StartsWith("Invariant Language", StringComparison.Ordinal))
        {
            throw new ArgumentOutOfRangeException(nameof(culture));
        }

        var region = new RegionInfo(culture.Name);

        Culture = culture;
        NativeName = region.CurrencyNativeName;
        EnglishName = region.CurrencyEnglishName;
        Symbol = region.CurrencySymbol;
        CurrencyIsoCode = region.ISOCurrencySymbol;
        DecimalPlaces = culture.NumberFormat.CurrencyDecimalDigits;
    }

    public Currency(
        string nativeName,
        string englishName,
        string symbol,
        string isoSymbol,
        int decimalDigits = DefaultDecimalDigits)
    {
        ThrowIfMissing(nativeName);
        ThrowIfMissing(englishName);
        ThrowIfMissing(symbol);
        ThrowIfMissing(isoSymbol);

        if (decimalDigits < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(decimalDigits),
                "Decimal Digits must be greater than or equal to zero.");
        }

        Culture = null;
        NativeName = nativeName;
        EnglishName = englishName;
        Symbol = symbol;
        CurrencyIsoCode = isoSymbol;
        DecimalPlaces = decimalDigits;
    }

    public bool Equals(Currency other) => Equals(other as ICurrency);

    public bool Equals(ICurrency other) =>
        other != null && CurrencyIsoCode.Equals(other.CurrencyIsoCode, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object obj) => obj is ICurrency other && Equals(other);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(CurrencyIsoCode);

    public override string ToString() => Symbol;

    public string ToString(decimal amount)
    {
        if (CurrencyIsoCode == "EUR")
        {
            return Symbol + amount.ToString("F" + DecimalPlaces, CultureInfo.InvariantCulture);
        }

        return Culture is null
            ? $"({CurrencyIsoCode}) {amount.ToString("N" + DecimalPlaces, CultureInfo.InvariantCulture)}"
            : amount.ToString("C" + DecimalPlaces, Culture);
    }

    public static bool operator ==(Currency left, Currency right) => left.Equals(right);

    public static bool operator !=(Currency left, Currency right) => !(left == right);

    private static void ThrowIfMissing(string argument, [CallerArgumentExpression(nameof(argument))] string name = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException($"{name} is required.", name);
        }
    }
}
