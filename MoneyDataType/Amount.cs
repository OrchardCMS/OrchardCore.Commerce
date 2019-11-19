using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using Money.Abstractions;
using Money.Serialization;

namespace Money
{
    /// <summary>
    /// A money amount, which is represented by a decimal number and a currency
    /// </summary>
    [JsonConverter(typeof(AmountConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(LegacyAmountConverter))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct Amount : IEquatable<Amount>, IComparable<Amount>
    {
        public Amount(decimal value, RegionInfo region)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));

            Currency = Money.Currency.FromRegion(region);
            Value = value;
        }

        public Amount(decimal value, CultureInfo culture)
        {
            if (culture is null)
                throw new ArgumentNullException(nameof(culture));

            Currency = Money.Currency.FromCulture(culture);
            Value = value;
        }

        /// <summary>
        /// Constructs a new money amount
        /// </summary>
        /// <param name="value">The decimal value</param>
        /// <param name="currency">The currency</param>
        public Amount(decimal value, ICurrency currency)
        {
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
            Value = value;
        }

        /// <summary>
        /// The decimal value
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// The currency
        /// </summary>
        public ICurrency Currency { get; }

        public bool Equals(Amount other) => Value == other.Value && Currency.Equals(other.Currency);

        public override bool Equals(object obj) => obj != null && obj is Amount other && Equals(other);

        public override int GetHashCode() => (Value, Currency).GetHashCode();

        public override string ToString() => Currency.ToString(Value);

        private string DebuggerDisplay => ToString();

        public int CompareTo(Amount other)
            => Currency != other.Currency
            ? throw new InvalidOperationException("Can't compare amounts of different currencies.")
            : Value.CompareTo(other.Value);

        public static explicit operator decimal(Amount amount) => amount.Value;

        public static Amount operator +(Amount first, Amount second)
            => first.Currency != second.Currency
            ? throw new InvalidOperationException("Can't add amounts of different currencies.")
            : new Amount(first.Value + second.Value, first.Currency);

        public static Amount operator -(Amount first, Amount second)
            => first.Currency != second.Currency
            ? throw new InvalidOperationException("Can't subtract amounts of different currencies.")
            : new Amount(first.Value - second.Value, first.Currency);

        public static Amount operator -(Amount amount)
            => new Amount(-amount.Value, amount.Currency);

        public static Amount operator *(int quantity, Amount amount)
            => new Amount(quantity * amount.Value, amount.Currency);

        public static Amount operator *(decimal quantity, Amount amount)
            => new Amount(quantity * amount.Value, amount.Currency);

        public static Amount operator *(float quantity, Amount amount)
            => new Amount((decimal)quantity * amount.Value, amount.Currency);

        public static Amount operator *(double quantity, Amount amount)
            => new Amount((decimal)quantity * amount.Value, amount.Currency);

        public static Amount operator *(Amount amount, int quantity)
            => new Amount(quantity * amount.Value, amount.Currency);

        public static Amount operator *(Amount amount, decimal quantity)
            => new Amount(quantity * amount.Value, amount.Currency);

        public static Amount operator *(Amount amount, float quantity)
            => new Amount((decimal)quantity * amount.Value, amount.Currency);

        public static Amount operator *(Amount amount, double quantity)
            => new Amount((decimal)quantity * amount.Value, amount.Currency);

        public static bool operator ==(Amount first, Amount second) => first.Equals(second);

        public static bool operator !=(Amount first, Amount second) => !first.Equals(second);

        public static bool operator <(Amount first, Amount second)
            => first.Currency != second.Currency
            ? throw new InvalidOperationException("Can't compare amounts of different currencies.")
            : first.Value < second.Value;

        public static bool operator >(Amount first, Amount second)
            => first.Currency != second.Currency
            ? throw new InvalidOperationException("Can't compare amounts of different currencies.")
            : first.Value > second.Value;

        public static bool operator <=(Amount first, Amount second)
            => first.Currency != second.Currency
            ? throw new InvalidOperationException("Can't compare amounts of different currencies.")
            : first.Value <= second.Value;

        public static bool operator >=(Amount first, Amount second)
            => first.Currency != second.Currency
            ? throw new InvalidOperationException("Can't compare amounts of different currencies.")
            : first.Value >= second.Value;
    }
}