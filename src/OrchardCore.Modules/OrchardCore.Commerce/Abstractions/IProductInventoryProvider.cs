using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Contains methods for inventory management.
/// </summary>
public interface IProductInventoryProvider : ISortableUpdaterProvider<IList<ShoppingCartItem>>
{
    /// <summary>
    /// Returns the current count of all inventories.
    /// </summary>
    Task<IDictionary<string, int>> QueryAllInventoriesAsync(string sku);

    /// <summary>
    /// Returns the current count of a specific inventory.
    /// </summary>
    Task<int> QueryInventoryAsync(string sku, string fullSku = null);
}
