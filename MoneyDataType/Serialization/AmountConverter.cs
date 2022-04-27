using Money.Abstractions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Money.Currency;

namespace Money.Serialization;

internal class AmountConverter : JsonConverter<Amount>
{
    public const string ValueName = "value";
    public const string CurrencyName = "currency";
    public const string Name = "name";
    public const string NativeName = "nativename";
    public const string EnglishName = "englishname";
    public const string Symbol = "symbol";
    public const string Iso = "iso";
    public const string DecimalDigits = "dec";

    public override Amount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        decimal value = default;
        ICurrency currency = null;
        string nativeName = null;
        string englishName = null;
        string symbol = null;
        string iso = null;
        int? decimalDigits = null;

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var propertyName = reader.GetString();
            if (!reader.Read()) continue;

            switch (propertyName)
            {
                case ValueName:
                    value = reader.GetDecimal();
                    break;
                case CurrencyName:
                    currency = Currency.FromIsoCode(reader.GetString());
                    break;
                case Name: // Kept for backwards compatibility
                case NativeName:
                    nativeName = reader.GetString();
                    break;
                case EnglishName:
                    englishName = reader.GetString();
                    break;
                case Symbol:
                    symbol = reader.GetString();
                    break;
                case Iso:
                    iso = reader.GetString();
                    break;
                case DecimalDigits when reader.TryGetInt32(out var i):
                    decimalDigits = i;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property name \"{propertyName}\".");
            }
        }

        if (currency is null) throw new InvalidOperationException("Invalid amount format. Must include a currency.");

        if (!Currency.IsKnownCurrency(currency.CurrencyIsoCode ?? string.Empty))
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

    public override void Write(Utf8JsonWriter writer, Amount amount, JsonSerializerOptions options)
    {
        if (amount.Currency is null)
        {
            throw new InvalidOperationException("Amount must have a currency applied to allow serialization.");
        }

        writer.WriteStartObject();
        writer.WriteNumber(ValueName, amount.Value);

        if (Currency.IsKnownCurrency(amount.Currency.CurrencyIsoCode))
        {
            writer.WriteString(CurrencyName, amount.Currency.CurrencyIsoCode);
        }
        else
        {
            writer.WriteString(NativeName, amount.Currency.NativeName);
            writer.WriteString(EnglishName, amount.Currency.EnglishName);
            writer.WriteString(Symbol, amount.Currency.Symbol);
            writer.WriteString(Iso, amount.Currency.CurrencyIsoCode);

            if (amount.Currency.DecimalPlaces != DefaultDecimalDigits)
            {
                writer.WriteNumber(DecimalDigits, amount.Currency.DecimalPlaces);
            }
        }

        writer.WriteEndObject();
    }
}
