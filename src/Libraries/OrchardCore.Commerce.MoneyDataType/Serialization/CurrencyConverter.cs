using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.MoneyDataType.Serialization;

internal sealed class CurrencyConverter : JsonConverter<ICurrency>
{
    public override ICurrency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Currency.FromIsoCode(reader.GetString());

    public override void Write(Utf8JsonWriter writer, ICurrency value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.CurrencyIsoCode);
}
