using System;
using System.Globalization;
using Newtonsoft.Json;

namespace OrchardCore.Commerce.Money
{
    internal class CurrencyConverter : JsonConverter<Currency>
    {
        public override Currency ReadJson(JsonReader reader, Type objectType, Currency existingValue, bool hasExistingValue, JsonSerializer serializer)
            => new Currency(null, null, (string)reader.Value, (CultureInfo)null, 2);

        public override void WriteJson(JsonWriter writer, Currency value, JsonSerializer serializer)
            => writer.WriteValue(value.IsoCode);
    }
}