using System;
using System.Diagnostics;
using Newtonsoft.Json;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Money
{
    /// <summary>
    /// A money amount, which is represented by a decimal number and a currency
    /// </summary>
    [JsonConverter(typeof(AmountConverter))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct Amount : IEquatable<Amount>, IComparable<Amount>
    {
        /// <summary>
        /// Constructs a new money amount
        /// </summary>
        /// <param name="value">The decimal value</param>
        /// <param name="currency">The currency</param>
        public Amount(decimal value, ICurrency currency)
        {
            Value = value;
            Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        }

        /// <summary>
        /// Constructs a new money amount
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="currency">The currency</param>
        public Amount(float value, ICurrency currency) : this((decimal)value, currency) { }
        /// <summary>
        /// Constructs a new money amount
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="currency">The currency</param>
        public Amount(double value, ICurrency currency) : this((decimal)value, currency) { }
        /// <summary>
        /// Constructs a new money amount
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="currency">The currency</param>
        public Amount(int value, ICurrency currency) : this((decimal)value, currency) { }

        /// <summary>
        /// The decimal value
        /// </summary>
        public decimal Value { get; }
        /// <summary>
        /// The currency
        /// </summary>
        public ICurrency Currency { get; }

        public bool Equals(Amount other) => Value == other.Value && Currency == other.Currency;

        public override bool Equals(object obj) => obj != null && obj is Amount other && Equals(other);

        public override int GetHashCode() => (Value, Currency).GetHashCode();

        public override string ToString() => (Currency ?? Money.Currency.Dollar).ToString(Value);

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