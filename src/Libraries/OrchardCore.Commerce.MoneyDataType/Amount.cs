#nullable enable

using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Serialization;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.MoneyDataType;

/// <summary>
/// A money amount, which is represented by a decimal number and a currency.
/// </summary>
[JsonConverter(typeof(AmountConverter))]
[Newtonsoft.Json.JsonConverter(typeof(LegacyAmountConverter))]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct Amount : IEquatable<Amount>, IComparable<Amount>
{
    public static Amount Unspecified { get; } = new(0, MoneyDataType.Currency.UnspecifiedCurrency);

    private readonly ICurrency? _currency;

    private string DebuggerDisplay => ToString();

    /// <summary>
    /// Gets the decimal value.
    /// </summary>
    public decimal Value { get; }

    /// <summary>
    /// Gets the currency.
    /// </summary>
    public ICurrency Currency => _currency ?? MoneyDataType.Currency.UnspecifiedCurrency;

    /// <summary>
    /// Gets a value indicating whether this <see cref="Amount"/> has usable values.
    /// </summary>
    public bool IsValid =>
        Value >= 0 &&
        Currency.CurrencyIsoCode != MoneyDataType.Currency.UnspecifiedCurrency.CurrencyIsoCode;

    public bool IsValidAndNonZero => Value > 0 && IsValid;

    [Obsolete($"Use {nameof(Unspecified)} instead.")]
    public Amount()
        : this(0, MoneyDataType.Currency.UnspecifiedCurrency)
    {
    }

    public Amount(decimal value, RegionInfo region)
        : this(value, MoneyDataType.Currency.FromRegion(region))
    {
    }

    public Amount(decimal value, CultureInfo culture)
        : this(value, MoneyDataType.Currency.FromCulture(culture))
    {
    }

    public Amount(decimal value, ICurrency currency)
    {
        _currency = currency;
        Value = value;
    }

    public bool Equals(Amount other) =>
        Value == other.Value &&
        Currency.Equals(other.Currency);

    public override bool Equals(object? obj) => obj is Amount other && Equals(other);

    public override int GetHashCode() => (Value, Currency).GetHashCode();

    public override string ToString() => Currency.ToString(Value);

    public int CompareTo(Amount other)
    {
        ThrowIfCurrencyDoesntMatch(other);
        return Value.CompareTo(other.Value);
    }

    private void ThrowIfCurrencyDoesntMatch(Amount other, string operation = "compare")
    {
        if (Currency.Equals(other.Currency)) return;
        throw new InvalidOperationException(
            $"Can't {operation} amounts of different currencies ({Currency.CurrencyIsoCode} and " +
            $"{other.Currency.CurrencyIsoCode}).");
    }

    #region Operators

    public static explicit operator decimal(Amount amount) => amount.Value;

    public static Amount operator +(Amount first, Amount second)
    {
        first.ThrowIfCurrencyDoesntMatch(second, operation: "add");
        return new Amount(first.Value + second.Value, first.Currency);
    }

    public static Amount operator -(Amount first, Amount second)
    {
        first.ThrowIfCurrencyDoesntMatch(second, operation: "subtract");
        return new Amount(first.Value - second.Value, first.Currency);
    }

    public static Amount operator -(Amount amount) =>
        new(-amount.Value, amount.Currency);

    public static Amount operator *(int quantity, Amount amount) =>
        new(quantity * amount.Value, amount.Currency);

    public static Amount operator *(decimal quantity, Amount amount) =>
        new(quantity * amount.Value, amount.Currency);

    public static Amount operator *(float quantity, Amount amount) =>
        new((decimal)quantity * amount.Value, amount.Currency);

    public static Amount operator *(double quantity, Amount amount) =>
        new((decimal)quantity * amount.Value, amount.Currency);

    public static Amount operator *(Amount amount, int quantity) =>
        new(quantity * amount.Value, amount.Currency);

    public static Amount operator *(Amount amount, decimal quantity) =>
        new(quantity * amount.Value, amount.Currency);

    public static Amount operator *(Amount amount, float quantity) =>
        new((decimal)quantity * amount.Value, amount.Currency);

    public static Amount operator *(Amount amount, double quantity) =>
        new((decimal)quantity * amount.Value, amount.Currency);

    public static Amount operator /(Amount amount, int quantity) =>
        new(amount.Value / quantity, amount.Currency);

    public static Amount operator /(Amount amount, decimal quantity) =>
        new(amount.Value / quantity, amount.Currency);

    public static Amount operator /(Amount amount, float quantity) =>
        amount / (decimal)quantity;

    public static Amount operator /(Amount amount, double quantity) =>
        amount / (decimal)quantity;

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

    #endregion
}
