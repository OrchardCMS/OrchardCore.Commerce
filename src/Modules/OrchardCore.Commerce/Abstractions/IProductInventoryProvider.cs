using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Contains inventory management related methods.
/// </summary>
public interface IProductInventoryProvider : ISortableUpdaterProvider<IList<ShoppingCartItem>>
{
    /// <summary>
    /// Returns the current inventory count.
    /// </summary>
    Task<int> QueryInventoryAsync(string sku);
}
