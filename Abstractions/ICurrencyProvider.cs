using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions
{
    /// <summary>
    /// A service that provides a set of currencies
    /// </summary>
    public interface ICurrencyProvider
    {
        /// <summary>
        /// The list of currencies
        /// </summary>
        IEnumerable<ICurrency> Currencies { get; }
        /// <summary>
        /// Finds the currency object for the provided symbol 
        /// </summary>
        /// <param name="isoSymbol">The three-letter ISO 4217 code for the currency</param>
        /// <returns>The currency object</returns>
        ICurrency GetCurrency(string isoSymbol);

        Task AddOrUpdateAsync(ICurrency currency);

        Task RemoveAsync(ICurrency currency);
    }
}
