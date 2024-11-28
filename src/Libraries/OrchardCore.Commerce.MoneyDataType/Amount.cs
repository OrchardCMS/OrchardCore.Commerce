#nullable enable

using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.MoneyDataType.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.MoneyDataType;

/// <summary>
/// A money amount, which is represented by a decimal number and a currency.
/// </summary>
[JsonConverter(typeof(AmountConverter))]
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

    public Amount(decimal value, CultureInfo? culture)
        : this(value, MoneyDataType.Currency.FromCulture(culture))
    {
    }

    public Amount(decimal value, ICurrency? currency)
    {
        _currency = currency ?? MoneyDataType.Currency.UnspecifiedCurrency;
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

    public Amount GetRounded() =>
        new(Math.Round(Value, Currency.DecimalPlaces), Currency);

    /// <summary>
    /// Converts the <see cref="Amount"/> to a fixed-point fractional value by keeping some digits based on the <see
    /// cref="ICurrency.CurrencyIsoCode"/>.
    /// </summary>
    /// <param name="roundingByCurrencyCode">
    /// Provides exceptional rounding rules for currencies that aren't converted according to the default. The key is
    /// the <see cref="Currency"/>'s ISO code, the value pairs follow the same logic as the matching default parameters.
    /// </param>
    /// <param name="defaultKeepDigits">Indicates how many digits should be kept after the decimal point.</param>
    /// <param name="defaultRoundTens">
    /// If positive, the <see cref="Amount"/> is rounded to this many digits before converted to a fixed-point
    /// fractional. Ignored otherwise.
    /// </param>
    public long GetFixedPointAmount(
        IDictionary<string, (int KeepDigits, int RoundTens)> roundingByCurrencyCode,
        int defaultKeepDigits = 2,
        int defaultRoundTens = 0)
    {
        static int Tens(int zeroes) => (int)Math.Pow(10, zeroes);

        var (keepDigits, roundTens) = roundingByCurrencyCode.TryGetValue(Currency.CurrencyIsoCode, out var pair)
            ? pair
            : (defaultKeepDigits, defaultRoundTens);

        return roundTens > 0
            ? (long)Math.Round(Value / Tens(roundTens)) * Tens(roundTens + keepDigits)
            : (long)Math.Round(Value * Tens(keepDigits));
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
        return new(first.Value + second.Value, first.Currency);
    }

    public static Amount operator +(Amount first, decimal second) => new(first.Value + second, first.Currency);

    public static Amount operator -(Amount first, Amount second)
    {
        first.ThrowIfCurrencyDoesntMatch(second, operation: "subtract");
        return new(first.Value - second.Value, first.Currency);
    }

    public static Amount operator -(Amount first, decimal second) => new(first.Value - second, first.Currency);

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
