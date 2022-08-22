using System;

namespace OrchardCore.Commerce.MoneyDataType.Abstractions;

/// <summary>
/// Currency representation.
/// </summary>
public interface ICurrency : IEquatable<ICurrency>
{
    /// <summary>
    /// Gets the symbol for the currency, usually a single character.
    /// </summary>
    string Symbol { get; }

    /// <summary>
    /// Gets the full native name of the currency.
    /// </summary>
    string NativeName { get; }

    /// <summary>
    /// Gets the full english name of the currency.
    /// </summary>
    string EnglishName { get; }

    /// <summary>
    /// Gets the three-letter ISO 4217 code for the currency if it exists (for non-standardized crypto-currencies for
    /// example, follow usage).
    /// </summary>
    string CurrencyIsoCode { get; }

    /// <summary>
    /// Gets the number of significant decimal places after the decimal separator.
    /// </summary>
    int DecimalPlaces { get; }

    /// <summary>
    /// Formats an amount of the currency.
    /// </summary>
    /// <param name="amount">The amount.</param>
    /// <returns>The formatted amount of the currency.</returns>
    string ToString(decimal amount);
}
