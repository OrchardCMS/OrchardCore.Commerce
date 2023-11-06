using Newtonsoft.Json;
using OrchardCore.Commerce.Serialization;

namespace OrchardCore.Commerce.ProductAttributeValues;

/// <summary>
/// Used only to deserialize attributes, before they're post-processed into concrete attribute values.
/// </summary>
[JsonConverter(typeof(LegacyRawProductAttributeValueConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(RawProductAttributeValueConverter))]
internal sealed class RawProductAttributeValue : BaseProductAttributeValue<object>
{
    public RawProductAttributeValue(object value)
        : base(attributeName: null, value)
    {
    }

    internal void SetAttributeName(string name) => AttributeName = name;
}
