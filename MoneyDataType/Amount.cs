using Money.Abstractions;
using Money.Serialization;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Money;

/// <summary>
/// A money amount, which is represented by a decimal number and a currency.
/// </summary>
[JsonConverter(typeof(AmountConverter))]
[Newtonsoft.Json.JsonConverter(typeof(LegacyAmountConverter))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct Amount : IEquatable<Amount>, IComparable<Amount>
{
    public Amount()
    {
        Value = 0;
        Currency = Money.Currency.UnspecifiedCurrency;
    }

    public Amount(decimal value, RegionInfo region)
    {
        if (region == null) throw new ArgumentNullException(nameof(region));

        Currency = Money.Currency.FromRegion(region);
        Value = value;
    }

    public Amount(decimal value, CultureInfo culture)
    {
        if (culture is null) throw new ArgumentNullException(nameof(culture));

        Currency = Money.Currency.FromCulture(culture);
        Value = value;
    }

    public Amount(decimal value, ICurrency currency)
    {
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        Value = value;
    }

    /// <summary>
    /// Gets the decimal value.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public ICurrency Currency { get; }

    public bool Equals(Amount other) =>
        Value == other.Value &&
        ((Currency == null && other.Currency == null) || Currency?.Equals(other.Currency) == true);

    public override bool Equals(object obj) => obj is Amount other && Equals(other);

    public override int GetHashCode() => (Value, Currency).GetHashCode();

    public override string ToString() => Currency?.ToString(Value);

    private string DebuggerDisplay => ToString();

    public int CompareTo(Amount other)
    {
        ThrowIfCurrencyDoesntMatch(other);
        return Value.CompareTo(other.Value);
    }

    public static explicit operator decimal(Amount amount) => amount.Value;

    public static Amount operator +(Amount first, Amount second)
    {
        first.ThrowIfCurrencyDoesntMatch(second, activity: "add");
        return new Amount(first.Value + second.Value, first.Currency);
    }

    public static Amount operator -(Amount first, Amount second)
    {
        first.ThrowIfCurrencyDoesntMatch(second, activity: "add");
        return new Amount(first.Value - second.Value, first.Currency);
    }

    public static Amount operator -(Amount amount)
        => new(-amount.Value, amount.Currency);

    public static Amount operator *(int quantity, Amount amount)
        => new(quantity * amount.Value, amount.Currency);

    public static Amount operator *(decimal quantity, Amount amount)
        => new(quantity * amount.Value, amount.Currency);

    public static Amount operator *(float quantity, Amount amount)
        => new((decimal)quantity * amount.Value, amount.Currency);

    public static Amount operator *(double quantity, Amount amount)
        => new((decimal)quantity * amount.Value, amount.Currency);

    public static Amount operator *(Amount amount, int quantity)
        => new(quantity * amount.Value, amount.Currency);

    public static Amount operator *(Amount amount, decimal quantity)
        => new(quantity * amount.Value, amount.Currency);

    public static Amount operator *(Amount amount, float quantity)
        => new((decimal)quantity * amount.Value, amount.Currency);

    public static Amount operator *(Amount amount, double quantity)
        => new((decimal)quantity * amount.Value, amount.Currency);

    public static bool operator ==(Amount first, Amount second) => first.Equals(second);

    public static bool operator !=(Amount first, Amount second) => !first.Equals(second);

    public static bool operator <(Amount first, Amount second)
    {
        first.ThrowIfCurrencyDoesntMatch(second);
        return first.Value < second.Value;
    }

    public static bool operator >(Amount first, Amount second)
    {
        first.ThrowIfCurrencyDoesntMatch(second);
        return first.Value > second.Value;
    }

    public static bool operator <=(Amount first, Amount second)
    {
        first.ThrowIfCurrencyDoesntMatch(second);
        return first.Value <= second.Value;
    }

    public static bool operator >=(Amount first, Amount second)
    {
        first.ThrowIfCurrencyDoesntMatch(second);
        return first.Value >= second.Value;
    }

    private void ThrowIfCurrencyDoesntMatch(Amount other, string activity = "compare")
    {
        if (Currency.Equals(other.Currency)) return;
        throw new InvalidOperationException(
            $"Can't {activity} amounts of different currencies ({Currency.CurrencyIsoCode} and {other.Currency.CurrencyIsoCode}).");
    }
}
