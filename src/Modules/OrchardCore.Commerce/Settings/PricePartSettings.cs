using System.Text.Json.Serialization;
using OrchardCore.Commerce.ContentFields.Settings;

namespace OrchardCore.Commerce.Settings;

public class PricePartSettings
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CurrencySelectionMode CurrencySelectionMode { get; set; }

    public string SpecificCurrencyIsoCode { get; set; }
}
