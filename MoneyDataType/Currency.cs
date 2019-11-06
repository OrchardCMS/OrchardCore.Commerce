using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System;
using System.Text.Json.Serialization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Serialization;

namespace OrchardCore.Commerce.Money
{
    [JsonConverter(typeof(CurrencyConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(LegacyCurrencyConverter))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct Currency : ICurrency
    {
        private readonly CultureInfo _culture;

        public Currency(string cultureCode)
            :this (CultureInfo.GetCultureInfo(cultureCode))
        { }

        internal Currency(CultureInfo culture)
        {
            if (culture is null)
                throw new ArgumentNullException(nameof(culture));
            if (culture.EnglishName.StartsWith("Unknown Locale") || culture.EnglishName.StartsWith("Invariant Language"))
                throw new ArgumentOutOfRangeException(nameof(culture));

            var region = new RegionInfo(culture.LCID);
            if (region is null)
                throw new ArgumentNullException(nameof(region));


            EnglishName = region.CurrencyEnglishName;
            NativeName = region.CurrencyNativeName;
            Symbol = region.CurrencySymbol;
            IsoCode = region.ISOCurrencySymbol;
            DecimalPlaces = culture.NumberFormat.CurrencyDecimalDigits;
            _culture = culture;
        }

        public Currency(string englishName, string nativeName, string symbol, string iSOSymbol, int decimalDigits = 2) 
        {
            EnglishName = englishName;
            NativeName = nativeName;
            Symbol = symbol;
            IsoCode = iSOSymbol;
            DecimalPlaces = decimalDigits;
            _culture = null;
        }

        public string Culture => _culture?.Name??IsoCode;

        public int DecimalPlaces { get; }

        public string IsoCode { get; }

        public string Symbol { get; }

        public string EnglishName { get; }

        public string NativeName { get; }

        public bool Equals(ICurrency other) => other != null && IsoCode == other.IsoCode;

        public override bool Equals(object obj) => obj != null && obj is ICurrency other && Equals(other);

        public override int GetHashCode() => IsoCode.GetHashCode();

        public override string ToString() => Symbol;

        public string ToString(decimal amount)
            => _culture!=null? amount.ToString("C" + DecimalPlaces, _culture) : "(" + (IsoCode ?? "UNK") + ") " + amount.ToString($"N{DecimalPlaces}");

        private string DebuggerDisplay => IsoCode;

        public static bool operator ==(Currency left, Currency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Currency left, Currency right)
        {
            return !(left == right);
        }

        public static ICurrency USDollar => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.USD);

        public static ICurrency Euro => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.EUR);

        public static ICurrency Yen => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.JPY);

        public static ICurrency PoundSterling => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.GBP);

        public static ICurrency AustralianDollar => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.AUD);

        public static ICurrency CanadianDollar => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.CAD);

        public static ICurrency SwissFranc => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.CHF);

        public static ICurrency Renminbi => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.CNY);

        public static ICurrency SwedishKrona => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.SEK);

        public static ICurrency BitCoin => KnownCurrencyTable.FromSystemCurrency(SystemCurrency.BTC);

        public static ICurrency FromSystemCurrency(SystemCurrency currency)
        {
            return FromISOCode(currency.ToString());
        }

        public static ICurrency FromISOCode(string code)
        {
            KnownCurrencyTable.EnsureCurrencyTable();
            return KnownCurrencyTable.CurrencyTable.FirstOrDefault(c => c.IsoCode == code);
        }

        public static ICurrency FromEnglishName(string name)
        {
            KnownCurrencyTable.EnsureCurrencyTable();
            return KnownCurrencyTable.CurrencyTable.FirstOrDefault(c => c.EnglishName == name);
        }

        public static ICurrency FromRegion(RegionInfo region)
        {
            KnownCurrencyTable.EnsureCurrencyTable();
            return KnownCurrencyTable.CurrencyTable.FirstOrDefault(c => c.IsoCode == region.ISOCurrencySymbol);
        }

        public static ICurrency FromCulture(CultureInfo culture)
        {
            KnownCurrencyTable.EnsureCurrencyTable();
            return KnownCurrencyTable.CurrencyTable.FirstOrDefault(c => c.Culture == culture.Name);
        }
    }
}
