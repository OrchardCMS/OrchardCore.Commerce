using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Serialization;

namespace OrchardCore.Commerce.Money
{
    public static class AmountExtensions
    {
        //public static Amount FromCulture(this decimal value, CultureInfo culture)
        //{
        //    if(  culture==null)  throw new ArgumentNullException(nameof(culture));
        //    if (culture.IsNeutralCulture) throw new ArgumentException($"Culture ID {culture.LCID} is a neutral culture; only specific cultures are supported.");

        //    Value = value;
        //    Region = new RegionInfo(culture.LCID);

        //    var currency = new Currency(Region.CurrencyNativeName, Region.CurrencySymbol, Region.ThreeLetterISORegionName, Culture, Culture.NumberFormat.CurrencyDecimalDigits);


        //}


    }
}
