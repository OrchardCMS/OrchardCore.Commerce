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
        IDictionary<string, JToken> propertiesAndValues = new Dictionary<string, JToken>();
        while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
        {
            var propertyName = reader.Value.ToString();
            reader.Read();
            var propertyValue = JToken.ReadFrom(reader);

            propertiesAndValues.Add(propertyName, propertyValue);
        }

        var attributeName = propertiesAndValues[AttributeName].ToString();

        return propertiesAndValues[Type].ToString() switch
        {
            nameof(TextProductAttributeValue) => new TextProductAttributeValue(attributeName, propertiesAndValues[Value].ToString()),
            nameof(BooleanProductAttributeValue) => new BooleanProductAttributeValue(attributeName, propertiesAndValues[Value].ToObject<bool>()),
            nameof(NumericProductAttributeValue) => new NumericProductAttributeValue(attributeName, propertiesAndValues[Value].ToObject<decimal>()),
            _ => null,
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
