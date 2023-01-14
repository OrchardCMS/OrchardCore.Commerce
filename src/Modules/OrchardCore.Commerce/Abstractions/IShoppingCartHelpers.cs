using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service to work with shopping cart data.
/// </summary>
public interface IShoppingCartHelpers
{
    /// <summary>
    /// Creates a model from the current shopping cart. This includes everything except the <see cref="IShape"/>
    /// collection in <see cref="ShoppingCartViewModel.TableShapes"/>.
    /// </summary>
    Task<ShoppingCartViewModel> CreateShoppingCartViewModelAsync(
        string shoppingCartId,
        Address shipping = null,
        Address billing = null);

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
