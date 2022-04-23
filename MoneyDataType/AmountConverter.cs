using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Money.Abstractions;

namespace Money;

internal class AmountConverter : JsonConverter<Amount>
{
    private const string ValueName = "value";
    private const string CurrencyName = "currency";
    private const string Name = "name";
    private const string NativeName = "nativename";
    private const string EnglishName = "englishname";
    private const string Symbol = "symbol";
    private const string Iso = "iso";
    private const string Dec = "dec";

    public override Amount Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = default(decimal);
        ICurrency currency = null;
        string nativeName = null;
        string englishName = null;
        string symbol = null;
        string iso = null;
        int? dec = null;

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
                case Dec when reader.TryGetInt32(out var i):
                    dec = i;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown property name \"{propertyName}\".");
            }
        }

        if (!Currency.IsKnownCurrency(currency?.CurrencyIsoCode ?? string.Empty))
        {
            currency = new Currency(nativeName, englishName, symbol, iso, dec.GetValueOrDefault(2));
        }

        if (currency is null) throw new InvalidOperationException("Invalid amount format. Must include a currency");

        return new Amount(value, currency);
    }

    public override void Write(Utf8JsonWriter writer, Amount amount, JsonSerializerOptions options)
    {
        if (amount.Currency is null)
        {
            throw new InvalidOperationException("Amount must have a currency applied to allow serialization");
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
            if (amount.Currency.DecimalPlaces != 2) writer.WriteNumber(Dec, amount.Currency.DecimalPlaces);
        }

        writer.WriteEndObject();
    }
}
