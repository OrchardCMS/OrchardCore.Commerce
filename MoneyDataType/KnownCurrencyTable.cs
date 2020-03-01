using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Money.Abstractions;

namespace Money
{
    internal static class KnownCurrencyTable
    {
        private static readonly object Obj = new object();

        internal static IDictionary<string, ICurrency> CurrencyTable { get; private set; }

        internal static void EnsureCurrencyTable()
        {
            if (CurrencyTable == null)
                InitCurrencyCodeTable();
        }

        private class CurrencyEqualityComparer : IEqualityComparer<ICurrency>
        {
            public bool Equals(ICurrency x, ICurrency y) => x.CurrencyIsoCode == y.CurrencyIsoCode;
            public int GetHashCode(ICurrency obj) => obj.CurrencyIsoCode.GetHashCode();
        }

        private static void InitCurrencyCodeTable()
        {
            lock (Obj)
            {
                bool valid(CultureInfo c) => !c.IsNeutralCulture && !c.EnglishName.StartsWith("Unknown Locale") && !c.EnglishName.StartsWith("Invariant Language");

                CurrencyTable = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Where(valid)
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
}
