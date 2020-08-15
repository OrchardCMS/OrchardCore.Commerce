using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OrchardCore.Commerce.Settings
{
    [JsonObject]
    public class PricePartSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencySelectionModeEnum CurrencySelectionMode { get; set; }
        public string SpecificCurrencyIsoCode { get; set; }
    }
}
