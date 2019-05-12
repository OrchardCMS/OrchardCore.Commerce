using System;

namespace OrchardCore.Commerce.Abstractions
{
    /// <summary>
    /// Currency representation
    /// </summary>
    public interface ICurrency : IEquatable<ICurrency>
    {
        /// <summary>
        /// The symbol for the currency, usually a single character
        /// </summary>
        string Symbol { get; }
        /// <summary>
        /// The full name of the currency
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The three-letter ISO 4217 code for the currency if it exists
        /// (for non-standardized crypto-currencies for example, follow usage)
        /// </summary>
        string IsoCode { get; }
        /// <summary>
        /// The number of significant decimal places after the decimal separator
        /// </summary>
        int DecimalPlaces { get; }
        /// <summary>
        /// A technical field that specifies whether the currency object has complete information
        /// </summary>
        bool IsResolved { get; }

        /// <summary>
        /// Formats an amount of the currency
        /// </summary>
        /// <param name="amount">The amount</param>
        /// <returns>The formatted amount of the currency</returns>
        string ToString(decimal amount);
    }
}
