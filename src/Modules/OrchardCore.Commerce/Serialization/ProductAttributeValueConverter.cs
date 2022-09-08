using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.ProductAttributeValues;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Serialization;

internal class ProductAttributeValueConverter : JsonConverter<IProductAttributeValue>
{
    private const string Type = "type";
    private const string Value = "value";
    private const string AttributeName = "attributename";

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
            nameof(TextProductAttributeValue) => new TextProductAttributeValue(attributeName, attribute.Get<string>(Value)),
            nameof(BooleanProductAttributeValue) => new BooleanProductAttributeValue(attributeName, attribute.Get<bool>(Value)),
            nameof(NumericProductAttributeValue) => new NumericProductAttributeValue(attributeName, attribute.Get<decimal>(Value)),
            _ => throw new InvalidOperationException($"Unknown or unsupported type \"{typeName}\"."),
        };
    }

    public override void WriteJson(JsonWriter writer, IProductAttributeValue productAttributeValue, JsonSerializer serializer)
    {
        if (productAttributeValue is null)
        {
            return;
        }

        writer.WriteStartObject();

        var typeName = productAttributeValue.GetType().Name;

        writer.WritePropertyName(Type);
        writer.WriteValue(typeName);

        writer.WritePropertyName(Value);

        if (typeName == nameof(TextProductAttributeValue))
        {
            writer.WriteValue((productAttributeValue.UntypedValue as IEnumerable<string>).FirstOrDefault());
        }
        else
        {
            writer.WriteValue(productAttributeValue.UntypedValue);
        }

        writer.WritePropertyName(AttributeName);
        writer.WriteValue(productAttributeValue.AttributeName);

        writer.WriteEndObject();
    }
}
