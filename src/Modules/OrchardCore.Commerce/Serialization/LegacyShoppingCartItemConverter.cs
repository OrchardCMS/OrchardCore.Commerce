using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Serialization;

internal sealed class LegacyShoppingCartItemConverter : JsonConverter<ShoppingCartItem>
{
    public override void WriteJson(JsonWriter writer, ShoppingCartItem value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(ShoppingCartItemConverter.QuantityName);
        writer.WriteValue(value.Quantity);

        writer.WritePropertyName(ShoppingCartItemConverter.SkuName);
        writer.WriteValue(value.ProductSku);

        if (value.Prices?.Any() == true)
        {
            writer.WritePropertyName(ShoppingCartItemConverter.PricesName);
            serializer.Serialize(writer, value.Prices);
        }

        if (value.Prices?.Any() == true)
        {
            writer.WritePropertyName(ShoppingCartItemConverter.AttributesName);
            serializer.Serialize(writer, value.Attributes.ToDictionary(
                attribute => attribute.AttributeName,
                attribute => attribute.UntypedValue));
        }

        writer.WriteEndObject();
    }

    public override ShoppingCartItem ReadJson(
        JsonReader reader,
        Type objectType,
        ShoppingCartItem existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var quantity = 1;
        string sku = null;
        ISet<IProductAttributeValue> attributes = new HashSet<IProductAttributeValue>();
        IList<PrioritizedPrice> prices = null;

        var properties = (JToken.ReadFrom(reader) as JObject)?.Properties() ?? Enumerable.Empty<JProperty>();
        foreach (var property in properties)
        {
            var value = property.Value;
            if (value.Type is JTokenType.Null or JTokenType.Undefined) continue;

            switch (property.Name)
            {
                case ShoppingCartItemConverter.QuantityName:
                    quantity = value.Value<int>();
                    break;
                case ShoppingCartItemConverter.SkuName:
                    sku = value.Value<string>();
                    break;
                case ShoppingCartItemConverter.PricesName:
                    prices = value.ToObject<List<PrioritizedPrice>>();
                    break;
                case ShoppingCartItemConverter.AttributesName:
                    foreach (var (name, attribute) in value.ToObject<Dictionary<string, RawProductAttributeValue>>())
                    {
                        attribute.SetAttributeName(name);
                        attributes.Add(attribute);
                    }

                    break;
                default:
                    throw new InvalidOperationException($"Unknown property name \"{property.Name}\".");
            }
        }

        return new ShoppingCartItem(quantity, sku, attributes, prices);
    }
}
