using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service that can update the inventories of a list of shopping cart items.
/// </summary>
public interface IProductInventoryService
{
    /// <summary>
    /// Updates inventories of shopping cart items using all inventory providers in the specified order.
    /// </summary>
    /// <param name="items">The quantities and products whose inventories need to be adjusted.</param>
    Task<IList<ShoppingCartItem>> UpdateInventoriesAsync(IList<ShoppingCartItem> items);

    /// <summary>
    /// Verifies the inventory state of the provided <paramref name="lines"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the line items can't be checked out.</returns>
    Task<bool> VerifyLinesAsync(IList<ShoppingCartLineViewModel> lines);
}
