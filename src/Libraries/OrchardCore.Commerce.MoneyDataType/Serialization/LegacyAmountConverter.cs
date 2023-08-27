using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
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
        var token = JToken.Load(reader);
        if (token.Type == JTokenType.String) return ReadString(token.Value<string>());

        var attribute = (JObject)token;

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

    public static Amount ReadString(string text)
    {
        var parts = text.Split();
        if (parts.Length < 2) throw new InvalidOperationException($"Unable to parse string amount \"{text}\".");

        var currency = FromIsoCode(parts[0]);
        var value = decimal.Parse(string.Join(string.Empty, parts.Skip(1)), CultureInfo.InvariantCulture);

        return new Amount(value, currency);
    }
}
