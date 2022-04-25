using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Price providers add prices to shopping cart items.
///
/// </summary>
public interface IPriceProvider
{
    /// <summary>
    /// Adds prices to shopping cart items.
    /// </summary>
    /// <param name="items">The quantities and products to which prices must be added.</param>
    Task<IEnumerable<ShoppingCartItem>> AddPricesAsync(IEnumerable<ShoppingCartItem> items);

    /// <summary>
    /// Gets the value used to sort price providers in ascending order. The first one that declares a price will be
    /// used.
    /// </summary>
    int Order { get; }
}
