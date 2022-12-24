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

    ///// <summary>
    ///// Updates the inventory of a product based on the provided parameters.
    ///// </summary>
    ///// <param name="difference">The value to add to or subtract from the inventory.</param>
    //void UpdateInventory(ProductPart productPart, int difference);
}
