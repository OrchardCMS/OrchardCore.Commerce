using Newtonsoft.Json;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;

namespace OrchardCore.Commerce.MoneyDataType.Serialization;

internal sealed class LegacyCurrencyConverter : JsonConverter<ICurrency>
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
