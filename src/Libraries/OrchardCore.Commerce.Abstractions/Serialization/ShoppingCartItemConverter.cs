using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ProductAttributeValues;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Abstractions.Serialization;

internal sealed class ShoppingCartItemConverter : JsonConverter<ShoppingCartItem>
{
    public const string QuantityName = "quantity";
    public const string SkuName = "sku";
    public const string PricesName = "prices";
    public const string AttributesName = "attributes";

    public override ShoppingCartItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var quantity = 1;
        string sku = null;
        ISet<IProductAttributeValue> attributes = null;
        IList<PrioritizedPrice> prices = null;

        foreach (var (propertyName, value) in JsonNode.Parse(ref reader)!.AsObject())
        {
            if (value is null) continue;

            switch (propertyName)
            {
                case QuantityName:
                    quantity = value.GetValue<int>();
                    break;
                case SkuName:
                    sku = value.GetValue<string>();
                    break;
                case PricesName:
                    prices = value.Deserialize<List<PrioritizedPrice>>();
                    break;
                case AttributesName:
                    attributes = ReadAttributeName(value);
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

    private static HashSet<IProductAttributeValue> ReadAttributeName(JsonNode node)
    {
        var attributes = new HashSet<IProductAttributeValue>();
        if (node is not JsonObject jsonObject) return attributes;

        foreach (var (name, value) in jsonObject)
        {
            if (value?.Deserialize<RawProductAttributeValue>() is { } attribute)
            {
                attribute.SetAttributeName(name);
                attributes.Add(attribute);
            }
        }

        return attributes;
    }
}
