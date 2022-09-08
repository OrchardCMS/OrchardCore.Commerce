using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service to work with shopping cart data.
/// </summary>
public interface IShoppingCartHelpers
{
    /// <summary>
    /// Calculate the total value in the cart. All prices must be of a single currency.
    /// </summary>
    /// <returns>The total value of the items in the cart, or <see langword="null" /> if the cart is empty.</returns>
    Task<Amount?> CalculateSingleCurrencyTotalAsync();

    /// <summary>
    /// Groups the line items in the cart by currency and returns the value by currency code.
    /// </summary>
    Task<IDictionary<string, Amount>> CalculateMultipleCurrencyTotalsAsync();
}
