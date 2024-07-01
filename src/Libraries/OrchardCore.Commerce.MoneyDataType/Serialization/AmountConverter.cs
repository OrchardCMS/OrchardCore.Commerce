using OrchardCore.Commerce.MoneyDataType.Abstractions;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static OrchardCore.Commerce.MoneyDataType.Currency;

namespace OrchardCore.Commerce.MoneyDataType.Serialization;

public sealed class AmountConverter : JsonConverter<Amount>
{
    public const string ValueName = "value";
    public const string CurrencyName = "currency";
    public const string Name = "name";
    public const string NativeName = "nativename"; // #spell-check-ignore-line
    public const string EnglishName = "englishname"; // #spell-check-ignore-line
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

        if (reader.TokenType == JsonTokenType.String) return ReadString(reader.GetString());

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
                    currency = FromIsoCode(reader.GetString());
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

        currency = HandleUnknownCurrency(currency, nativeName, englishName, symbol, iso, decimalDigits);

        return new(value, currency);
    }

    public override void Write(Utf8JsonWriter writer, Amount amount, JsonSerializerOptions options)
    {
        if (amount.Currency is null)
        {
            throw new InvalidOperationException("Amount must have a currency applied to allow serialization.");
        }

        writer.WriteStartObject();
        writer.WriteNumber(ValueName, amount.Value);

        if (IsKnownCurrency(amount.Currency.CurrencyIsoCode))
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

    private static ICurrency HandleUnknownCurrency(
        ICurrency currency,
        string nativeName,
        string englishName,
        string symbol,
        string iso,
        int? decimalDigits)
    {
        if (!IsKnownCurrency(currency?.CurrencyIsoCode ?? string.Empty))
        {
            return new Currency(
                nativeName,
                englishName,
                symbol,
                iso,
                decimalDigits.GetValueOrDefault(DefaultDecimalDigits));
        }

        if (currency != null) return currency;

        throw new InvalidOperationException($"Invalid amount format. Must include a {nameof(currency)}.");
    }

    public static Amount ReadString(string text)
    {
        var parts = text.Split();
        if (parts.Length < 2) throw new InvalidOperationException($"Unable to parse string amount \"{text}\".");

        var currency = FromIsoCode(parts[0]);
        var value = decimal.Parse(string.Join(string.Empty, parts.Skip(1)), CultureInfo.InvariantCulture);

        return new(value, currency);
    }
}
