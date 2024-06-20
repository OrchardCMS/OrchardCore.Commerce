using System.Text.Json.Serialization;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// A simple product price.
/// </summary>
public class PricePart : ContentPart
{
    [JsonIgnore]
    public Amount Price
    {
        get => PriceField.Amount;
        set => PriceField.Amount = value;
    }

    public PriceField PriceField { get; set; } = new();
}
