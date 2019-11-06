using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Money
{
    internal static class KnownCurrencyTable
    {
        private static object _obj = new object();

        internal static IDictionary<string, ICurrency> CurrencyTable { get; private set; }

        internal static void EnsureCurrencyTable()
        {
            if (CurrencyTable == null)
                InitCurrencyCodeTable();
        }

        private class CurrencyEqualityComparer : IEqualityComparer<ICurrency>
        {
            public bool Equals(ICurrency x, ICurrency y) => x.IsoCode == y.IsoCode;
            public int GetHashCode(ICurrency obj) => obj.IsoCode.GetHashCode();
        }

        private static void InitCurrencyCodeTable()
        {
            lock (_obj)
            {
                bool valid(CultureInfo c) => !c.IsNeutralCulture && !c.EnglishName.StartsWith("Unknown Locale") && !c.EnglishName.StartsWith("Invariant Language");

                CurrencyTable = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Where(valid)
                    .Select(c => new Currency(c)).Cast<ICurrency>()
                    .Distinct(new CurrencyEqualityComparer())
                    .ToDictionary(k => k.IsoCode, e => e);

                CurrencyTable.Add("BTC", new Currency("BitCoin", "₿", "BTC", 8));
            }
        }

        internal static ICurrency FromIsoCode(string isoCode)
        {
            EnsureCurrencyTable();
            return CurrencyTable[isoCode];
        }
    }
}
