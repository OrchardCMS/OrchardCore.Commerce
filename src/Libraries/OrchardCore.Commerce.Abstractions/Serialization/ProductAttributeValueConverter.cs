using OrchardCore.Commerce.Abstractions.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Abstractions.Serialization;

public sealed class ProductAttributeValueConverter : JsonConverter<IProductAttributeValue>
{
    private const string TypePropertyName = "type";
    private const string ValuePropertyName = "value";
    private const string AttributeNamePropertyName = "attributeName";

    public override IProductAttributeValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new InvalidOperationException(
                $"Expected object when deserializing {typeToConvert.Name}, got {reader.TokenType}");
        }

        var attribute = JsonNode.Parse(ref reader)!.AsObject();
        var attributeName = attribute[AttributeNamePropertyName]?.GetValue<string>();
        var typeName = attribute[TypePropertyName]?.GetValue<string>();

        var deserializer = IProductAttributeDeserializer.Deserializers.GetMaybe(typeName) ??
            throw new InvalidOperationException($"Unknown or unsupported type \"{typeName}\".");

        return deserializer.Deserialize(attributeName, attribute);
    }

    public override void Write(Utf8JsonWriter writer, IProductAttributeValue value, JsonSerializerOptions options)
    {
        if (value is null) return;

        writer.WriteStartObject();

        writer.WriteString(TypePropertyName, value.GetType().Name);
        writer.WriteString(AttributeNamePropertyName, value.AttributeName);

        writer.WritePropertyName(ValuePropertyName);
        var valueNode = value.UntypedValue is IEnumerable<object> values
            ? JArray.FromObject(values, options)
            : JNode.FromObject(value.UntypedValue, options);
        valueNode?.WriteTo(writer, options);

        writer.WriteEndObject();
    }
}
