using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;

namespace OrchardCore.Commerce.Serialization;

internal class ShoppingCartItemConverter : JsonConverter<ShoppingCartItem>
{
    private const string QuantityName = "quantity";
    private const string SkuName = "sku";
    private const string PricesName = "prices";
    private const string AttributesName = "attributes";

    public override ShoppingCartItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var quantity = 1;
        string sku = null;
        ISet<IProductAttributeValue> attributes = null;
        IList<PrioritizedPrice> prices = null;
        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var propertyName = reader.GetString();
            if (!reader.Read()) continue;

            switch (propertyName)
            {
                case QuantityName:
                    quantity = reader.GetInt32();
                    break;
                case SkuName:
                    sku = reader.GetString();
                    break;
                case PricesName:
                    prices = JsonSerializer.Deserialize<List<PrioritizedPrice>>(ref reader);
                    break;
                case AttributesName:
                    attributes = ReadAttributeName(reader);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property name \"{propertyName}\".");
            }
        }

        return new ShoppingCartItem(quantity, sku, attributes, prices);
    }

    public override void Write(Utf8JsonWriter writer, ShoppingCartItem value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(QuantityName, value.Quantity);
        writer.WriteString(SkuName, value.ProductSku);
        if (value.Prices != null)
        {
            writer.WritePropertyName(PricesName);
            JsonSerializer.Serialize(writer, value.Prices, options);
        }

        if (value.Attributes != null)
        {
            writer.WriteStartObject(AttributesName);
            foreach (var attribute in value.Attributes)
            {
                writer.WritePropertyName(attribute.AttributeName);
                // Re-using the raw attribute serialization logic
                JsonSerializer.Serialize(writer, new RawProductAttributeValue(attribute.UntypedValue), options);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

    private static HashSet<IProductAttributeValue> ReadAttributeName(Utf8JsonReader reader)
    {
        var attributes = new HashSet<IProductAttributeValue>();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName) continue;
            var attributeName = reader.GetString();
            // It looks like a .NET Core bug that I have to do that, but whatevs. It's for "perf", or so Fowler tells me.
            var value = JsonSerializer.Deserialize<RawProductAttributeValue>(ref reader) ??
                new RawProductAttributeValue(value: null);
            value.SetAttributeName(attributeName);
            attributes.Add(value);
        }

        return attributes;
    }
}
