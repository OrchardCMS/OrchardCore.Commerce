using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Money
{
    [JsonConverter(typeof(CurrencyConverter))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Currency : ICurrency
    {
        public static readonly ICurrency Dollar = new Currency("Dollar", "$", "USD", "en-US");
        public static readonly ICurrency Euro = new Currency("Euro", "€", "EUR", "fr-FR");
        public static readonly ICurrency Yen = new Currency("Yen", "¥", "JPY", "jp-JP", 0);
        public static readonly ICurrency PoundSterling = new Currency("Pound sterling", "£", "GBP", "en-GB");
        public static readonly ICurrency AustralianDollar = new Currency("Australian dollar", "$", "AUD", "en-AU");
        public static readonly ICurrency CanadianDollar = new Currency("Canadian dollar", "$", "CAD", "en-CA");
        public static readonly ICurrency SwissFranc = new Currency("Swiss Franc", "CHF", "CHF", "rm-CH");
        public static readonly ICurrency Renminbi = new Currency("Renminbi", "¥", "CNY", "zh-Hans");
        public static readonly ICurrency BitCoin = new BitCoin();

        public Currency(string name, string symbol, string isoCode, CultureInfo culture, int decimalPlaces = 2)
        {
            Name = name;
            Symbol = symbol;
            IsoCode = isoCode;
            Culture = culture;
            DecimalPlaces = decimalPlaces;
        }

        public Currency(string name, string symbol, string isoCode, string cultureCode, int decimalPlaces = 2)
            : this(name, symbol, isoCode, CultureInfo.GetCultureInfo(cultureCode), decimalPlaces) { }


        public string Symbol { get; }

        public string Name { get; }

        public string IsoCode { get; }

        private CultureInfo Culture { get; }

        public int DecimalPlaces { get; }

        public bool IsResolved => Name != null;

        public bool Equals(ICurrency other) => other != null && IsoCode == other.IsoCode;

        public override bool Equals(object obj) => obj != null && obj is ICurrency other && Equals(other);

        public override int GetHashCode() => IsoCode.GetHashCode();

        public override string ToString() => Symbol;

        public virtual string ToString(decimal amount)
            => IsResolved ? amount.ToString("C" + DecimalPlaces, Culture) : "(" + (IsoCode ?? "UNK") + ") " + amount.ToString("N2");

        private string DebuggerDisplay => IsResolved ? Name : IsoCode;
    }

    public sealed class BitCoin : Currency
    {
        // Note: The BTC symbol is most used, but official Iso code would be "XBT"
        internal BitCoin() : base("BitCoin", "₿", "BTC", (CultureInfo)null, 8) { }
        
        public override string ToString(decimal amount) => amount.ToString("N8") + " BTC";
    }
}
