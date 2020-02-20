using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Money;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Serialization
{
    internal class PrioritizedPriceConverter : JsonConverter<PrioritizedPrice>
    {
        private const string PriorityName = "priority";
        private const string AmountName = "amount";

        public override PrioritizedPrice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var priority = Int32.MinValue;
            var amount = new Amount(0, Currency.UnspecifiedCurrency);

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName) break;

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
                }
            }

            if (priority > int.MinValue && amount.Currency != Currency.UnspecifiedCurrency)
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
}