using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Money
{
    public struct BitCoin : ICurrency
    {
        public string Symbol => "₿";
        public string EnglishName => "BitCoin";
        public string NativeName => "BitCoin";
        public string IsoCode => "BTC";
        public int DecimalPlaces => 8;

        public string Culture => "BTC";

        public bool Equals(ICurrency other) => other.IsoCode == IsoCode;

        public string ToString(decimal amount) => amount.ToString($"N{DecimalPlaces}") + " BTC";
    }
}
