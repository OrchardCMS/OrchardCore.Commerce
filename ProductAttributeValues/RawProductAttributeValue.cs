using System.Text.Json.Serialization;
using OrchardCore.Commerce.Serialization;

namespace OrchardCore.Commerce.ProductAttributeValues
{
    /// <summary>
    /// Used only to deserialize attributes, before they're post-processed into concrete attribute values.
    /// </summary>
    [JsonConverter(typeof(RawProductAttributeValueConverter))]
    internal class RawProductAttributeValue : BaseProductAttributeValue<object>
    {
        public RawProductAttributeValue(object value)
            : base(null, value) { }

        public void SetAttributeName(string name) => AttributeName = name;
    }
}
