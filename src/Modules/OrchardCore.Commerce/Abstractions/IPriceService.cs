using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service that can add prices to a set of shopping cart items.
/// </summary>
public interface IPriceService
{
    /// <summary>
    /// Adds prices harvested from all price providers to shopping cart items, in order.
    /// </summary>
    /// <param name="items">The quantities and products to which prices must be added.</param>
    Task<IList<ShoppingCartItem>> AddPricesAsync(IList<ShoppingCartItem> items);
}
