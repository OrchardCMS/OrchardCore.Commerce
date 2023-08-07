using OrchardCore.Commerce.Serialization;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.ProductAttributeValues;

/// <summary>
/// Used only to deserialize attributes, before they're post-processed into concrete attribute values.
/// </summary>
[JsonConverter(typeof(RawProductAttributeValueConverter))]
internal sealed class RawProductAttributeValue : BaseProductAttributeValue<object>
{
    public RawProductAttributeValue(object value)
        : base(attributeName: null, value)
    {
    }

    public void SetAttributeName(string name) => AttributeName = name;
}
