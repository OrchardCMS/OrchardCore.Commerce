using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using System;

namespace OrchardCore.Commerce.Abstractions.Serialization;

internal sealed class LegacyPrioritizedPriceConverter : JsonConverter<PrioritizedPrice>
{
    public override void WriteJson(JsonWriter writer, PrioritizedPrice value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(PrioritizedPriceConverter.PriorityName);
        writer.WriteValue(value.Priority);

        writer.WritePropertyName(PrioritizedPriceConverter.AmountName);
        serializer.Serialize(writer, value.Price);

        writer.WriteEndObject();
    }

    public override PrioritizedPrice ReadJson(
        JsonReader reader,
        Type objectType,
        PrioritizedPrice existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var priority = int.MinValue;
        var amount = Amount.Unspecified;
        var token = JToken.ReadFrom(reader);

        if ((token as JObject)?.Properties() is { } properties)
        {
            foreach (var property in properties)
            {
                switch (property.Name)
                {
                    case PrioritizedPriceConverter.PriorityName:
                        priority = property.Value.Value<int>();
                        break;
                    case PrioritizedPriceConverter.AmountName:
                        amount = property.Value.ToObject<Amount>();
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown property name \"{property.Name}\".");
                }
            }
        }
        else if (token.Type == JTokenType.String)
        {
            amount = token.ToObject<Amount>();
        }

        return new PrioritizedPrice(priority, amount);
    }
}
