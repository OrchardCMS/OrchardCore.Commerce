using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrchardCore.Commerce.ContentFields.Settings;

namespace OrchardCore.Commerce.Settings;

[JsonObject]
public class PricePartSettings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public CurrencySelectionMode CurrencySelectionMode { get; set; }

    public string SpecificCurrencyIsoCode { get; set; }
}
