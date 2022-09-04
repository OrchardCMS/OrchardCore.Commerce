using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.ContentManagement;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// A simple product price.
/// </summary>
public class PricePart : ContentPart
{
    public Amount Price
    {
        get => PriceField.Amount;
        set => PriceField.Amount = value;
    }

    public PriceField PriceField { get; set; } = new();

    // Prevents JSON.Net serialization, see https://stackoverflow.com/a/24224465/14748624 for details.
    [SuppressMessage("Minor Code Smell", "S3400:Methods should not return constants", Justification = "JSON.Net")]
    public bool ShouldSerializePrice() => false;
}
