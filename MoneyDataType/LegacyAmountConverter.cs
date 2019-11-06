using System;
using System.Globalization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Serialization
{
    internal class LegacyAmountConverter : Newtonsoft.Json.JsonConverter<Amount>
    {
        private const string ValueName = "value";
        private const string CurrencyName = "currency";

        public override Amount ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, Amount existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var val = default(decimal);
            ICurrency currency = null;
            while (reader.Read())
            {
                if (reader.TokenType != Newtonsoft.Json.JsonToken.PropertyName) break;

                var propertyName = reader.Value;

                switch (propertyName)
                {
                    case ValueName:
                        val = (decimal)reader.ReadAsDecimal();
                        break;
                    case CurrencyName:
                        currency = Currency.FromISOCode(reader.ReadAsString());
                        break;
                }
            }

            if (currency is null)
                currency = Currency.FromCulture(CultureInfo.CurrentCulture);

            return new Amount(val, currency);
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, Amount amount, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (amount.Currency is null)
                throw new InvalidOperationException("Amount must have a currency applied to allow serialization");

            writer.WriteStartObject();
            writer.WritePropertyName(ValueName);
            writer.WriteValue(amount.Value);
            writer.WritePropertyName(CurrencyName);
            writer.WriteValue(amount.Currency.IsoCode);
            writer.WriteEndObject();
        }
    }
}