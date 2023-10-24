using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Serialization;

internal sealed class PrioritizedPriceConverter : JsonConverter<PrioritizedPrice>
{
    public const string PriorityName = "priority";
    public const string AmountName = "amount";

    public override PrioritizedPrice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var priority = int.MinValue;
        var amount = Amount.Unspecified;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var propertyName = reader.GetString();
            if (!reader.Read()) continue;

            switch (propertyName)
            {
                case PriorityName:
                    priority = reader.GetInt32();
                    break;
                case AmountName:
                    amount = JsonSerializer.Deserialize<Amount>(ref reader);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property name \"{propertyName}\".");
            }
        }

        if (priority > int.MinValue && !amount.Currency.Equals(Currency.UnspecifiedCurrency))
        {
            return new PrioritizedPrice(priority, amount);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, PrioritizedPrice prioritizedPrice, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(PriorityName, prioritizedPrice.Priority);
        writer.WritePropertyName(AmountName);
        JsonSerializer.Serialize(writer, prioritizedPrice.Price, options);
        writer.WriteEndObject();
    }
}
