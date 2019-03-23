using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Commerce.Money;

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
        /// Finds a currency from its ISO symbol.
        /// </summary>
        /// <param name="isoSymbol">The ISO symbol of the currency to look up.</param>
        /// <returns>The currency if found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if the currency symbol is not found from any active provider.</exception>
        ICurrency GetCurrency(string isoSymbol);

        /// <summary>
        /// Creates an amount object from a value and a currency symbol.
        /// </summary>
        /// <param name="value">The decimal value of the amount.</param>
        /// <param name="currencyIsoSymbol">The ISO symbol of the currency.</param>
        /// <returns>The amount.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if the currency symbol is not found from any active provider.</exception>
        Amount Create(decimal value, string currencyIsoSymbol);

        /// <summary>
        /// The currency to use or assume when none is provided.
        /// </summary>
        ICurrency DefaultCurrency { get; }

        /// <summary>
        /// Returns a new Amount where the currency has been verified to be resolved, or resolved.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        Amount EnsureCurrency(Amount amount);
    }
}
