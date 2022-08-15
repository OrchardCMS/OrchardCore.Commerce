using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;
using System.Text.Json;

namespace OrchardCore.Commerce.MoneyDataType.Serialization;

internal class CurrencyConverter : System.Text.Json.Serialization.JsonConverter<ICurrency>
{
    public override ICurrency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Currency.FromIsoCode(reader.GetString());

    public override void Write(Utf8JsonWriter writer, ICurrency value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.CurrencyIsoCode);
}
