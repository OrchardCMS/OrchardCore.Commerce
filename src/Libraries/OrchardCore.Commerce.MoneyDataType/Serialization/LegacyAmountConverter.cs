using Newtonsoft.Json;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;
using static OrchardCore.Commerce.MoneyDataType.Currency;
using static OrchardCore.Commerce.MoneyDataType.Serialization.AmountConverter;

namespace OrchardCore.Commerce.MoneyDataType.Serialization;

internal class LegacyAmountConverter : JsonConverter<Amount>
{
    public override Amount ReadJson(
        JsonReader reader,
        Type objectType,
        Amount existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        decimal value = default;
        ICurrency currency = null;
        string nativeName = null;
        string englishName = null;
        string symbol = null;
        string iso = null;
        int? decimalDigits = null;

        while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
        {
            var propertyName = reader.Value;

            switch (propertyName)
            {
                case ValueName:
                    value = reader.ReadAsDecimal().Value;
                    break;
                case CurrencyName:
                    currency = FromIsoCode(reader.ReadAsString());
                    break;
                case Name: // Kept for backwards compatibility
                case NativeName:
                    nativeName = reader.ReadAsString();
                    break;
                case EnglishName:
                    englishName = reader.ReadAsString();
                    break;
                case Symbol:
                    symbol = reader.ReadAsString();
                    break;
                case Iso:
                    iso = reader.ReadAsString();
                    break;
                case DecimalDigits:
                    decimalDigits = reader.ReadAsInt32();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property name \"{propertyName}\".");
            }
        }

        if (string.IsNullOrEmpty(currency?.CurrencyIsoCode)) return Amount.Unspecified;

        if (!IsKnownCurrency(currency.CurrencyIsoCode))
        {
            currency = new Currency(
                nativeName,
                englishName,
                symbol,
                iso,
                decimalDigits.GetValueOrDefault(DefaultDecimalDigits));
        }

        if (currency is null) throw new InvalidOperationException("Invalid amount format. Must include a currency.");

        return new Amount(value, currency);
    }

    public override void WriteJson(JsonWriter writer, Amount amount, JsonSerializer serializer)
    {
        if (amount.Currency is null)
        {
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName(ValueName);
        writer.WriteValue(amount.Value);

        if (IsKnownCurrency(amount.Currency.CurrencyIsoCode))
        {
            writer.WritePropertyName(CurrencyName);
            writer.WriteValue(amount.Currency.CurrencyIsoCode);
        }
        else
        {
            writer.WritePropertyName(NativeName);
            writer.WriteValue(amount.Currency.NativeName);
            writer.WritePropertyName(EnglishName);
            writer.WriteValue(amount.Currency.EnglishName);
            writer.WritePropertyName(Symbol);
            writer.WriteValue(amount.Currency.Symbol);
            writer.WritePropertyName(Iso);
            writer.WriteValue(amount.Currency.CurrencyIsoCode);
            if (amount.Currency.DecimalPlaces != DefaultDecimalDigits)
            {
                writer.WritePropertyName(DecimalDigits);
                writer.WriteValue(amount.Currency.DecimalPlaces);
            }
        }

        writer.WriteEndObject();
    }
}
