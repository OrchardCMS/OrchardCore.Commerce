using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Serialization
{
    internal class AmountConverter : JsonConverter<Amount>
    {
        private const string ValueName = "value";
        private const string CurrencyName = "currency";

        public override Amount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = default(decimal);
            ICurrency currency = null;

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName) break;

                var propertyName = reader.GetString();
                if (!reader.Read()) continue;

                switch (propertyName)
                {
                    case ValueName:
                        val = reader.GetDecimal();
                        break;
                    case CurrencyName:
                        currency = Currency.FromISOCode(reader.GetString());
                        break;
                }
            }

            if (currency is null)
                throw new InvalidOperationException("Invalid amount format. Must include a currency");

            return new Amount(val, currency);
        }

        public override void Write(Utf8JsonWriter writer, Amount amount, JsonSerializerOptions options)
        {
            if (amount.Currency is null)
                throw new InvalidOperationException("Amount must have a currency applied to allow serialization");
            writer.WriteStartObject();
            writer.WriteNumber(ValueName, amount.Value);
            writer.WriteString(CurrencyName, amount.Currency.IsoCode);
            writer.WriteEndObject();
        }
    }
}