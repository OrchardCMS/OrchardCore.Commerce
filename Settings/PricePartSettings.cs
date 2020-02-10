using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Commerce.Settings
{
    public class CurrencySelectionModes
    {
        public const string AllCurrencies = "AllCurrencies";
        public const string DefaultCurrency = "DefaultCurrency";
        public const string SpecificCurrency = "SpecificCurrency";
    }

    public class PricePartSettings
    {
        public string CurrencySelectionMode { get; set; }
        public string CurrencyIsoCode { get; set; }
    }
}
