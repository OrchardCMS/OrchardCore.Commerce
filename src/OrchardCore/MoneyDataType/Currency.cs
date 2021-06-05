using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using Money.Abstractions;
using Money.Serialization;

namespace Money
{
    [JsonConverter(typeof(CurrencyConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(LegacyCurrencyConverter))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public partial struct Currency : ICurrency
    {
        public Currency(CultureInfo culture)
        {
            if (culture is null)
                throw new ArgumentNullException(nameof(culture));
            if (culture.EnglishName.StartsWith("Unknown Locale") || culture.EnglishName.StartsWith("Invariant Language"))
                throw new ArgumentOutOfRangeException(nameof(culture));

            var region = new RegionInfo(culture.LCID);
            if (region is null)
                throw new ArgumentNullException(nameof(region));

            Culture = culture;
            NativeName = region.CurrencyNativeName;
            EnglishName = region.CurrencyEnglishName;
            Symbol = region.CurrencySymbol;
            CurrencyIsoCode = region.ISOCurrencySymbol;
            DecimalPlaces = culture.NumberFormat.CurrencyDecimalDigits;
        }

        public Currency(string nativename, string englishname, string symbol, string iSOSymbol, int decimalDigits = 2)
        {
            if (string.IsNullOrWhiteSpace(nativename))
                throw new ArgumentException("NativeName is required", nameof(nativename));
            if (string.IsNullOrWhiteSpace(englishname))
                throw new ArgumentException("EnglishName is required", nameof(englishname));
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol is required", nameof(symbol));
            if (string.IsNullOrWhiteSpace(iSOSymbol))
                throw new ArgumentException("ISO Symbol is required", nameof(iSOSymbol));
            if (decimalDigits < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalDigits), "Decimal Digits must be greater than or equal to zero");

            Culture = null;
            NativeName = nativename;
            EnglishName = englishname;
            Symbol = symbol;
            CurrencyIsoCode = iSOSymbol;
            DecimalPlaces = decimalDigits;
        }

        public int DecimalPlaces { get; }

        public string CurrencyIsoCode { get; }

        public string NativeName { get; }

        public string EnglishName { get; }

        public string Symbol { get; }

        public CultureInfo Culture { get; }

        public static ICurrency UnspecifiedCurrency = new Currency("Unspecified", "Unspecified", "---", "---");

        public bool Equals(ICurrency other) => other != null && CurrencyIsoCode.Equals(other.CurrencyIsoCode, StringComparison.InvariantCultureIgnoreCase);

        public override bool Equals(object obj) => obj != null && obj is ICurrency other && Equals(other);

        public override int GetHashCode() => CurrencyIsoCode.GetHashCode();

        public override string ToString() => Symbol;

        public string ToString(decimal amount)
            => Culture is null
            ? "(" + CurrencyIsoCode + ") " + amount.ToString("N" + DecimalPlaces)
            : amount.ToString("C" + DecimalPlaces, Culture);

        private string DebuggerDisplay
            => CurrencyIsoCode;

        public static bool operator ==(Currency left, Currency right)
            => left.Equals(right);

        public static bool operator !=(Currency left, Currency right)
            => !(left == right);
    }
}
