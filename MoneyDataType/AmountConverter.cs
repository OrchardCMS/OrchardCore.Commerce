using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Money.Abstractions;

namespace Money
{
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
            var val = default(decimal);
            ICurrency currency = null;
            string nativename = null;
            string englishname = null;
            string symbol = null;
            string iso = null;
            int? dec = null;

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName) break;

                var propertyName = reader.GetString();
                if (!reader.Read()) continue;

                switch (propertyName)
                {
                    case ValueName:
                        val = reader.GetDecimal();
                        break;
                    case CurrencyName:
                        currency = Currency.FromISOCode(reader.GetString());
                        break;

                    // Kept for backwards compatibility
                    case Name:
                        nativename = reader.GetString();
                        break;

                    case NativeName:
                        nativename = reader.GetString();
                        break;
                    case EnglishName:
                        englishname = reader.GetString();
                        break;
                    case Symbol:
                        symbol = reader.GetString();
                        break;
                    case Iso:
                        iso = reader.GetString();
                        break;
                    case Dec:
                        if (reader.TryGetInt32(out var i)) dec = i;
                        break;
                }
            }

            if (!Currency.IsKnownCurrency(currency?.CurrencyIsoCode ?? ""))
                currency = new Currency(nativename, englishname, symbol, iso, dec.GetValueOrDefault(2));

            if (currency is null)
                throw new InvalidOperationException("Invalid amount format. Must include a currency");

            return new Amount(val, currency);
        }

        public override void Write(Utf8JsonWriter writer, Amount amount, JsonSerializerOptions options)
        {
            if (amount.Currency is null)
                throw new InvalidOperationException("Amount must have a currency applied to allow serialization");
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
                if (amount.Currency.DecimalPlaces != 2)
                    writer.WriteNumber(Dec, amount.Currency.DecimalPlaces);
            }
            writer.WriteEndObject();
        }
    }
}
