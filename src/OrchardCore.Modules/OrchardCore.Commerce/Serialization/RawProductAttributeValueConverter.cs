using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Commerce.ProductAttributeValues;

namespace OrchardCore.Commerce.Serialization
{
    internal class RawProductAttributeValueConverter : JsonConverter<RawProductAttributeValue>
    {
        public override RawProductAttributeValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new RawProductAttributeValue(JsonSerializer.Deserialize<object>(ref reader, options));

        public override void Write(Utf8JsonWriter writer, RawProductAttributeValue value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value.UntypedValue, options);
    }
}
