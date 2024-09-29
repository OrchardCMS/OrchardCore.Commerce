using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Serialization;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Abstractions.Serialization;

public sealed class PrioritizedPriceConverter : JsonConverter<PrioritizedPrice>
{
    public const string PriorityName = "priority";
    public const string AmountName = "amount";

    public override PrioritizedPrice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var priority = int.MinValue;
        var amount = Amount.Unspecified;

        if (reader.TokenType == JsonTokenType.String)
        {
            return new(priority, AmountConverter.ReadString(reader.GetString()));
        }

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
            return new(priority, amount);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, PrioritizedPrice value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(PriorityName, value.Priority);
        writer.WritePropertyName(AmountName);
        JsonSerializer.Serialize(writer, value.Price, options);
        writer.WriteEndObject();
    }
}
