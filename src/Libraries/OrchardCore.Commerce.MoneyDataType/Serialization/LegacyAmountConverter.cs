using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using static OrchardCore.Commerce.MoneyDataType.Currency;
using static OrchardCore.Commerce.MoneyDataType.Serialization.AmountConverter;

namespace OrchardCore.Commerce.MoneyDataType.Serialization;

internal sealed class LegacyAmountConverter : JsonConverter<Amount>
{
    public override Amount ReadJson(
        JsonReader reader,
        Type objectType,
        Amount existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var attribute = (JObject)JToken.Load(reader);

        var value = attribute.Get<decimal>(ValueName);
        var currencyCode = attribute.Get<string>(CurrencyName);
        var nativeName = attribute.Get<string>(Name, NativeName);
        var englishName = attribute.Get<string>(EnglishName);
        var symbol = attribute.Get<string>(Symbol);
        var iso = attribute.Get<string>(Iso);
        var decimalDigits = attribute.Get<int?>(DecimalDigits);
        var currency = string.IsNullOrEmpty(currencyCode ?? iso) ? null : FromIsoCode(currencyCode ?? iso);

        var currencyIsEmpty = string.IsNullOrEmpty(currency?.CurrencyIsoCode);
        if (currencyIsEmpty && (string.IsNullOrEmpty(iso) || iso == UnspecifiedCurrency.CurrencyIsoCode))
        {
            return Amount.Unspecified;
        }

        if (currencyIsEmpty || !IsKnownCurrency(currency.CurrencyIsoCode))
        {
            currency = new Currency(
                nativeName,
                englishName,
                symbol,
                iso,
                decimalDigits.GetValueOrDefault(DefaultDecimalDigits));
        }

        return new Amount(value, currency);
    }

    public override void WriteJson(JsonWriter writer, Amount amount, JsonSerializer serializer)
    {
        if (amount.Currency is null) return;

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
