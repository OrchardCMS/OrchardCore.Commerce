using Newtonsoft.Json;
using OrchardCore.Commerce.ProductAttributeValues;
using System;

namespace OrchardCore.Commerce.Serialization;

internal sealed class LegacyRawProductAttributeValueConverter : JsonConverter<RawProductAttributeValue>
{
    public override void WriteJson(JsonWriter writer, RawProductAttributeValue value, JsonSerializer serializer) =>
        serializer.Serialize(writer, value.UntypedValue);

    public override RawProductAttributeValue ReadJson(
        JsonReader reader,
        Type objectType,
        RawProductAttributeValue existingValue,
        bool hasExistingValue,
        JsonSerializer serializer) =>
        new(serializer.Deserialize<object>(reader));
}
