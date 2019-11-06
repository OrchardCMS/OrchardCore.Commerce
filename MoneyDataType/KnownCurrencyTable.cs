using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using OrchardCore.Commerce.Abstractions;
using System;
using System.Diagnostics;

namespace OrchardCore.Commerce.Money
{
    internal static class KnownCurrencyTable
    {
        internal static ICurrency[] CurrencyTable;

        internal static void EnsureCurrencyTable()
        {
            if (CurrencyTable == null)
                InitCurrencyCodeTable();
        }

        private class CurrencyEqualityComparer : IEqualityComparer<ICurrency>
        {
            public bool Equals(ICurrency x, ICurrency y) => x.IsoCode == y.IsoCode;
            public int GetHashCode(ICurrency obj) => obj.GetHashCode();
        }
        private static object obj = new object();

        private class CultureLCIDComparer : IEqualityComparer<CultureInfo>
        {
            public bool Equals(CultureInfo x, CultureInfo y) => x.LCID == y.LCID;
            public int GetHashCode(CultureInfo obj) => obj.LCID.GetHashCode();
        }

        private static void InitCurrencyCodeTable()
        {
            lock (obj)
            {
                var currencies = new List<ICurrency>();
                bool valid(CultureInfo c) => !c.IsNeutralCulture && !c.EnglishName.StartsWith("Unknown Locale") && !c.EnglishName.StartsWith("Invariant Language");
                var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Where(valid)
                    .Distinct(new CultureLCIDComparer())
                    .ToList();

                currencies.AddRange(cultures.Select(c=> new Currency(c)).Cast<ICurrency>());

                CurrencyTable = currencies
                    .Distinct(new CurrencyEqualityComparer())
                    .OrderBy(c => c.IsoCode)
                    .ToArray();
            }
        }

        internal static ICurrency FromSystemCurrency(SystemCurrency currency)
        {
            EnsureCurrencyTable();
            return CurrencyTable.First(c => c.IsoCode == currency.ToString());
        }

        //private static CultureInfo SetCultureForRegion(RegionInfo region)
        //{
        //    try
        //    {
        //        var regionName = region.Name.ToLower();
        //        if (regionName.IndexOf('-') == -1) regionName = "-" + regionName;
        //        var r = CultureInfo.GetCultures(CultureTypes.SpecificCultures).FirstOrDefault(c => c.Name.ToLowerInvariant().IndexOf(regionName) > -1);
        //        return r ?? CultureInfo.GetCultureInfo(region.Name);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }
}
