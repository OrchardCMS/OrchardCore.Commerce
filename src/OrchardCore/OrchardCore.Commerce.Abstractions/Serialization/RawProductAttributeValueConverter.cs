using OrchardCore.Commerce.Abstractions.ProductAttributeValues;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Abstractions.Serialization;

internal sealed class RawProductAttributeValueConverter : JsonConverter<RawProductAttributeValue>
{
    public override RawProductAttributeValue Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        new(JsonSerializer.Deserialize<object>(ref reader, options));

    public override void Write(Utf8JsonWriter writer, RawProductAttributeValue value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.UntypedValue, options);
}
