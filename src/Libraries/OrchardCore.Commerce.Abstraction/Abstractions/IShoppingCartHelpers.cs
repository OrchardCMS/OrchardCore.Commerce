using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Exceptions;
using OrchardCore.Commerce.Models;
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
    /// Calculate the total value in the <paramref name="cart"/>. All prices must be of a single currency.
    /// </summary>
    /// <returns>The total value of the items in the cart, or <see langword="null" /> if the cart is empty.</returns>
    Task<Amount?> CalculateSingleCurrencyTotalAsync(ShoppingCart cart);

    /// <summary>
    /// Groups the line items in the <paramref name="cart"/> by currency and returns the value by currency code.
    /// </summary>
    Task<IDictionary<string, Amount>> CalculateMultipleCurrencyTotalsAsync(ShoppingCart cart);

    /// <summary>
    /// Adds a new entry to the shopping cart, optionally saves the cart using <c>IShoppingCartPersistence</c> if
    /// <paramref name="storeIfOk"/> is <see langword="true"/>.
    /// </summary>
    /// <exception cref="FrontendException">
    /// Thrown if the cart validation fails. Its <see cref="FrontendException.HtmlMessage"/> can be displayed safely.
    /// </exception>
    Task<ShoppingCartItem> AddToCartAsync(
        string shoppingCartId,
        ShoppingCartItem item,
        bool storeIfOk = false);

    /// <summary>
    /// Adds the product with the given <paramref name="sku"/> to the shopping cart without saving, validates the cart
    /// and calculates the display information for the added item.
    /// </summary>
    Task<ShoppingCartLineViewModel> EstimateProductAsync(
        string shoppingCartId,
        string sku,
        Address shipping = null,
        Address billing = null);

    /// <summary>
    /// Returns a <see cref="OrderLineItem"/> list from the given <paramref name="shoppingCart"/> items.
    /// </summary>
    Task<IList<OrderLineItem>> CreateOrderLineItemsAsync(ShoppingCart shoppingCart);
}
