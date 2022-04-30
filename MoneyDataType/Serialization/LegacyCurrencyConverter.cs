using Money.Abstractions;
using Newtonsoft.Json;
using System;

namespace Money.Serialization;

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
