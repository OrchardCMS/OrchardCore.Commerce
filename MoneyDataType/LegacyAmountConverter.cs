using System;
using Money.Abstractions;

namespace Money.Serialization
{
    internal class LegacyAmountConverter : Newtonsoft.Json.JsonConverter<Amount>
    {
        private const string ValueName = "value";
        private const string CurrencyName = "currency";

        private const string Name = "name";
        private const string Symbol = "symbol";
        private const string Iso = "iso";
        private const string Dec = "dec";

        public override Amount ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, Amount existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var val = default(decimal);
            ICurrency currency = null;
            string name = null;
            string symbol = null;
            string iso = null;
            int? dec = null;
            bool unknown = false;

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

                    case Name:
                        name = reader.ReadAsString();
                        unknown = true;
                        break;
                    case Symbol:
                        symbol = reader.ReadAsString();
                        unknown = true;
                        break;
                    case Iso:
                        iso = reader.ReadAsString();
                        unknown = true;
                        break;
                    case Dec:
                        dec = reader.ReadAsInt32();
                        unknown = true;
                        break;
                }
            }

            if (unknown)
                currency = new Currency(name, symbol, iso, dec.GetValueOrDefault(2));

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
            if (amount.Currency.IsKnownCurrency)
            {
                writer.WritePropertyName(CurrencyName);
                writer.WriteValue(amount.Currency.CurrencyIsoCode);
            }
            else
            {
                writer.WritePropertyName(Name);
                writer.WriteValue(amount.Currency.Name);
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