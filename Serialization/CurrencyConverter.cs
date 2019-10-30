using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.Commerce.Money;

namespace OrchardCore.Commerce.Serialization
{
    internal class CurrencyConverter : JsonConverter<Currency>
    {
        public override Currency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new Currency(null, null, reader.GetString(), (CultureInfo)null, 2);

        public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.IsoCode);
    }

    internal class LegacyCurrencyConverter : Newtonsoft.Json.JsonConverter<Currency>
    {
        public override Currency ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, Currency existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
            => new Currency(null, null, reader.ReadAsString(), (CultureInfo)null, 2);

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, Currency value, Newtonsoft.Json.JsonSerializer serializer)
            => writer.WriteValue(value.IsoCode);
    }
}