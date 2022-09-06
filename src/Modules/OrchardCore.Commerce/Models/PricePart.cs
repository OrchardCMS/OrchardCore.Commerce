using Newtonsoft.Json;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// A simple product price.
/// </summary>
public class PricePart : ContentPart
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Amount Price
    {
        get => PriceField.Amount;
        set => PriceField.Amount = value;
    }

    public PriceField PriceField { get; set; } = new();
}
