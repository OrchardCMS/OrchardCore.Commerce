using System;
using System.Collections.Generic;
using Money;
using Money.Abstractions;

namespace OrchardCore.Commerce.Abstractions
{
    /// <summary>
    /// Represents a set of utilities to more easily work with currency providers.
    /// </summary>
    public interface IMoneyService
    {
        /// <summary>
        /// Returns all currencies from all currency providers.
        /// </summary>
        IEnumerable<ICurrency> Currencies { get; }

        /// <summary>
        /// Finds a currency from its ISO code.
        /// </summary>
        /// <param name="currencyIsoCode">The ISO code of the currency to look up.</param>
        /// <returns>The currency if found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if the currency code is not found from any active provider.</exception>
        ICurrency GetCurrency(string currencyIsoCode);

        /// <summary>
        /// Creates an amount object from a value and a currency symbol.
        /// </summary>
        /// <param name="value">The decimal value of the amount.</param>
        /// <param name="currencyIsoCode">The ISO code of the currency.</param>
        /// <returns>The amount.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if the currency code is not found from any active provider.</exception>
        Amount Create(decimal value, string currencyIsoCode);

        /// <summary>
        /// The currency to use or assume when none is provided.
        /// </summary>
        ICurrency DefaultCurrency { get; }

        /// <summary>
        /// The current currency used for displaying prices to the customer.
        /// </summary>
        ICurrency CurrentDisplayCurrency { get; }

        /// <summary>
        /// Returns a new Amount where the currency has been verified to be resolved, or resolved.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        Amount EnsureCurrency(Amount amount);
    }
}
