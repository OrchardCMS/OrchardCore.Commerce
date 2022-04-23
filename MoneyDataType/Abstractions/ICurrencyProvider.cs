using System.Collections.Generic;

namespace Money.Abstractions;

/// <summary>
/// A service that provides a set of currencies.
/// </summary>
public interface ICurrencyProvider
{
    /// <summary>
    /// Gets the list of currencies.
    /// </summary>
    IEnumerable<ICurrency> Currencies { get; }

    /// <summary>
    /// Finds the currency object for the provided symbol.
    /// </summary>
    /// <param name="isoCode">The three-letter ISO 4217 code for the currency.</param>
    /// <returns>The currency object.</returns>
    ICurrency GetCurrency(string isoCode);

    /// <summary>
    /// Returns true if the <see cref="Currency.CurrencyIsoCode"/> is registered in
    /// this provider instance, otherwise false.
    /// </summary>
    bool IsKnownCurrency(string isoCode);
}
