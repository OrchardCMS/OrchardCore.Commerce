using System;
using Money.Abstractions;

namespace Money.Serialization
{
    internal class LegacyAmountConverter : Newtonsoft.Json.JsonConverter<Amount>
    {
        private const string ValueName = "value";
        private const string CurrencyName = "currency";

        private const string Name = "name";
        private const string NativeName = "nativename";
        private const string EnglishName = "englishname";
        private const string Symbol = "symbol";
        private const string Iso = "iso";
        private const string Dec = "dec";

        public override Amount ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, Amount existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
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
                if (reader.TokenType != Newtonsoft.Json.JsonToken.PropertyName) break;

                var propertyName = reader.Value;

                switch (propertyName)
                {
                    case ValueName:
                        val = (decimal)reader.ReadAsDecimal();
                        break;
                    case CurrencyName:
                        currency = Currency.FromISOCode(reader.ReadAsString());
                        break;

                    // Kept for backwards compatibility
                    case Name:
                        nativename = reader.ReadAsString();
                        break;

                    case NativeName:
                        nativename = reader.ReadAsString();
                        break;
                    case EnglishName:
                        englishname = reader.ReadAsString();
                        break;
                    case Symbol:
                        symbol = reader.ReadAsString();
                        break;
                    case Iso:
                        iso = reader.ReadAsString();
                        break;
                    case Dec:
                        dec = reader.ReadAsInt32();
                        break;
                }
            }

            if (!Currency.IsKnownCurrency(currency?.CurrencyIsoCode ?? ""))
                currency = new Currency(nativename, englishname, symbol, iso, dec.GetValueOrDefault(2));

            if (currency is null)
                throw new InvalidOperationException("Invalid amount format. Must include a currency");

            return new Amount(val, currency);
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, Amount amount, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (amount.Currency is null)
                throw new InvalidOperationException("Amount must have a currency applied to allow serialization");

            writer.WriteStartObject();
            writer.WritePropertyName(ValueName);
            writer.WriteValue(amount.Value);
            if (Currency.IsKnownCurrency(amount.Currency.CurrencyIsoCode))
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
                if (amount.Currency.DecimalPlaces != 2)
                {
                    writer.WritePropertyName(Dec);
                    writer.WriteValue(amount.Currency.DecimalPlaces);
                }
            }
            writer.WriteEndObject();
        }
    }
}
