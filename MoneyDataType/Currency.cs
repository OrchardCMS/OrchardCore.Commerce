using Money.Abstractions;
using Money.Serialization;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Money;

[JsonConverter(typeof(CurrencyConverter))]
[Newtonsoft.Json.JsonConverter(typeof(LegacyCurrencyConverter))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly partial struct Currency : ICurrency, IEquatable<Currency>
{
    public const int DefaultDecimalDigits = 2;

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
        if (string.IsNullOrWhiteSpace(nativeName))
        {
            throw new ArgumentException("NativeName is required.", nameof(nativeName));
        }

        if (string.IsNullOrWhiteSpace(englishName))
        {
            throw new ArgumentException("EnglishName is required.", nameof(englishName));
        }

        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol is required.", nameof(symbol));
        }

        if (string.IsNullOrWhiteSpace(isoSymbol))
        {
            throw new ArgumentException("ISO Symbol is required.", nameof(isoSymbol));
        }

        if (decimalDigits < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(decimalDigits), "Decimal Digits must be greater than or equal to zero.");
        }

        Culture = null;
        NativeName = nativeName;
        EnglishName = englishName;
        Symbol = symbol;
        CurrencyIsoCode = isoSymbol;
        DecimalPlaces = decimalDigits;
    }

    public int DecimalPlaces { get; }

    public string CurrencyIsoCode { get; }

    public string NativeName { get; }

    public string EnglishName { get; }

    public string Symbol { get; }

    public CultureInfo Culture { get; }

    public static ICurrency UnspecifiedCurrency { get; } = new Currency("Unspecified", "Unspecified", "---", "---");

    public bool Equals(Currency other) => Equals(other as ICurrency);

    public bool Equals(ICurrency other) =>
        other != null && CurrencyIsoCode.Equals(other.CurrencyIsoCode, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object obj) => obj is ICurrency other && Equals(other);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(CurrencyIsoCode);

    public override string ToString() => Symbol;

    public string ToString(decimal amount) =>
        Culture is null
            ? $"({CurrencyIsoCode}) {amount.ToString("N" + DecimalPlaces, CultureInfo.InvariantCulture)}"
            : amount.ToString("C" + DecimalPlaces, Culture);

    private string DebuggerDisplay => CurrencyIsoCode;

    public static bool operator ==(Currency left, Currency right) => left.Equals(right);

    public static bool operator !=(Currency left, Currency right) => !(left == right);
}
