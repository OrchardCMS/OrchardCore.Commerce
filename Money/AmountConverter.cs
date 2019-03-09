using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Commerce.Money
{
    internal class AmountConverter : JsonConverter<Amount>
    {
        public override Amount ReadJson(JsonReader reader, Type objectType, Amount existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var val = default(decimal);
            var currency = Currency.Dollar;
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName) break;

                var propertyName = (string)reader.Value;
                if (!reader.Read()) continue;

                switch(propertyName)
                {
                    case nameof(Amount.Value):
                        val = serializer.Deserialize<decimal>(reader);
                        break;
                    case nameof(Amount.Currency):
                        currency = new Currency(null, null, serializer.Deserialize<string>(reader), (CultureInfo)null);
                        break;
                }
            }

            return new Amount(val, currency);
        }

        public override void WriteJson(JsonWriter writer, Amount amount, JsonSerializer serializer)
        {
            var o = new JObject();
            o.Add(new JProperty(nameof(Amount.Value), amount.Value));
            if (amount.Currency?.IsoCode != null)
                o.Add(new JProperty(nameof(Amount.Currency), amount.Currency?.IsoCode ?? "USD"));
            o.WriteTo(writer);
        }
    }
}