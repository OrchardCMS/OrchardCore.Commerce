using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Money.Abstractions;

namespace Money.Serialization
{
    internal class CurrencyConverter : JsonConverter<ICurrency>
    {
        public override ICurrency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Currency.FromISOCode(reader.GetString());

        public override void Write(Utf8JsonWriter writer, ICurrency value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.CurrencyIsoCode);
    }

    internal class LegacyCurrencyConverter : Newtonsoft.Json.JsonConverter<ICurrency>
    {
        public override ICurrency ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, ICurrency existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
            => Currency.FromISOCode(reader.ReadAsString());

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, ICurrency value, Newtonsoft.Json.JsonSerializer serializer)
            => writer.WriteValue(value.CurrencyIsoCode);
    }
}