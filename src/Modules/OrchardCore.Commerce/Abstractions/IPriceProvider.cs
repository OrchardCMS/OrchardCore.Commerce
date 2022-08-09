using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Price providers add prices to shopping cart items.
/// </summary>
public interface IPriceProvider
{
    /// <summary>
    /// Gets the value used to sort price providers in ascending order. The first one that declares a price will be
    /// used.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Adds prices to shopping cart items.
    /// </summary>
    /// <param name="items">The quantities and products to which prices must be added.</param>
    Task<IEnumerable<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items);

    /// <summary>
    /// Adds price to shopping cart item.
    /// </summary>
    /// <param name="item">The quantity and product to which prices must be added.</param>
    Task<ShoppingCartItem> AddPriceAsync(ShoppingCartItem item);

    /// <summary>
    /// Checks whether or not the provider is applicable for the products.
    /// </summary>
    /// <param name="items">The quantities and products that needs to be checked.</param>
    Task<bool> IsApplicableAsync(IList<ShoppingCartItem> items);
}
