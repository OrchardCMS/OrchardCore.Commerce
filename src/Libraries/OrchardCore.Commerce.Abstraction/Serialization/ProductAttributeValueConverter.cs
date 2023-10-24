using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.ProductAttributeValues;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Serialization;

internal sealed class ProductAttributeValueConverter : JsonConverter<IProductAttributeValue>
{
    private const string Type = "type";
    private const string Value = "value";
    private const string AttributeName = "attributeName";

    public override IProductAttributeValue ReadJson(
        JsonReader reader,
        Type objectType,
        IProductAttributeValue existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var attribute = (JObject)JToken.Load(reader);

        var attributeName = attribute.Get<string>(AttributeName);
        var typeName = attribute.Get<string>(Type);

        return typeName switch
        {
            nameof(TextProductAttributeValue) => new TextProductAttributeValue(attributeName, attribute.Get<IEnumerable<string>>(Value)),
            nameof(BooleanProductAttributeValue) => new BooleanProductAttributeValue(attributeName, attribute.Get<bool>(Value)),
            nameof(NumericProductAttributeValue) => new NumericProductAttributeValue(attributeName, attribute.Get<decimal>(Value)),
            _ => throw new InvalidOperationException($"Unknown or unsupported type \"{typeName}\"."),
        };
    }

    public override void WriteJson(JsonWriter writer, IProductAttributeValue productAttributeValue, JsonSerializer serializer)
    {
        if (productAttributeValue is null) return;

        writer.WriteStartObject();
        writer.WritePropertyName(Type);
        writer.WriteValue(productAttributeValue.GetType().Name);

        var untypedValue = productAttributeValue.UntypedValue;

        writer.WritePropertyName(Value);

        if (untypedValue is IEnumerable<object> values)
        {
            writer.WriteStartArray();

            foreach (var value in values)
            {
                writer.WriteValue(value);
            }

            writer.WriteEndArray();
        }
        else
        {
            writer.WriteValue(untypedValue);
        }

        writer.WritePropertyName(AttributeName);
        writer.WriteValue(productAttributeValue.AttributeName);

        writer.WriteEndObject();
    }
}
