using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Serialization
{
    internal class AmountConverter : JsonConverter<Amount>
    {
        private const string valueName = "value";
        private const string currencyName = "currency";

        public override Amount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var val = default(decimal);
            var currency = Currency.FromCulture(CultureInfo.CurrentCulture);

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName) break;

                var propertyName = reader.GetString();
                if (!reader.Read()) continue;

                switch (propertyName)
                {
                    case valueName:
                        val = reader.GetDecimal();
                        break;
                    case currencyName:
                        currency = new Currency(null,null, null, reader.GetString());
                        break;
                }
            }
            return new Amount(val, currency);
        }

        public override void Write(Utf8JsonWriter writer, Amount amount, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(valueName, amount.Value);
            if (amount.Currency?.IsoCode != null)
            {
                writer.WriteString(currencyName, amount.Currency?.IsoCode ?? "USD");
            }
            writer.WriteEndObject();
        }
    }

    internal class LegacyAmountConverter : Newtonsoft.Json.JsonConverter<Amount>
    {
        private const string valueName = "value";
        private const string currencyName = "currency";

        public override Amount ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, Amount existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var val = default(decimal);
            var currency = Currency.FromCulture(CultureInfo.CurrentCulture);
            while (reader.Read())
            {
                if (reader.TokenType != Newtonsoft.Json.JsonToken.PropertyName) break;

                var propertyName = reader.Value;

                switch (propertyName)
                {
                    case valueName:
                        val = (decimal)reader.ReadAsDecimal();
                        break;
                    case currencyName:
                        currency = new Currency(null,null, null, reader.ReadAsString());
                        break;
                }
            }

            return new Amount(val, currency);
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, Amount amount, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(valueName);
            writer.WriteValue(amount.Value);
            if (amount.Currency?.IsoCode != null)
            {
                writer.WritePropertyName(currencyName);
                writer.WriteValue(amount.Currency?.IsoCode ?? "USD");
            }
            writer.WriteEndObject();
        }
    }
}