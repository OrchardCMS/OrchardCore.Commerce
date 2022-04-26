using Money.Abstractions;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Money.Serialization;

internal class CurrencyConverter : System.Text.Json.Serialization.JsonConverter<ICurrency>
{
    public override ICurrency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Currency.FromIsoCode(reader.GetString());

    public override void Write(Utf8JsonWriter writer, ICurrency value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.CurrencyIsoCode);
}

internal class LegacyCurrencyConverter : JsonConverter<ICurrency>
{
    public override ICurrency ReadJson(
        JsonReader reader,
        Type objectType,
        ICurrency existingValue,
        bool hasExistingValue,
        JsonSerializer serializer) =>
        Currency.FromIsoCode(reader.ReadAsString());

    public override void WriteJson(JsonWriter writer, ICurrency value, JsonSerializer serializer) =>
        writer.WriteValue(value.CurrencyIsoCode);
}
