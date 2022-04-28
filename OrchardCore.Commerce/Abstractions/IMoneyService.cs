using Money;
using Money.Abstractions;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Contains a set of utilities to more easily work with currency providers.
/// </summary>
public interface IMoneyService
{
    /// <summary>
    /// Gets all currencies from all currency providers.
    /// </summary>
    IEnumerable<ICurrency> Currencies { get; }

    /// <summary>
    /// Gets the currency to use or assume when none is provided.
    /// </summary>
    ICurrency DefaultCurrency { get; }

    /// <summary>
    /// Gets the current currency used for displaying prices to the customer.
    /// </summary>
    ICurrency CurrentDisplayCurrency { get; }

    /// <summary>
    /// Finds a currency from its ISO code.
    /// </summary>
    /// <param name="currencyIsoCode">The ISO code of the currency to look up.</param>
    /// <returns>The currency if found.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throws if the currency code is not found from any active provider.
    /// </exception>
    ICurrency GetCurrency(string currencyIsoCode);

    /// <summary>
    /// Creates an amount object from a value and a currency symbol.
    /// </summary>
    /// <param name="value">The decimal value of the amount.</param>
    /// <param name="currencyIsoCode">The ISO code of the currency.</param>
    /// <returns>The amount.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throws if the currency code is not found from any active provider.
    /// </exception>
    Amount Create(decimal value, string currencyIsoCode);

    /// <summary>
    /// Returns a new <see cref="Amount"/> that has been verified using <see cref="Currency.FromIsoCode"/> to ensure its
    /// currency is a known ISO 4217 code.
    /// </summary>
    Amount EnsureCurrency(Amount amount);
}
