using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

// contains business logic. what business logic tho
public interface IProductInventoryProvider : ISortableUpdaterProvider<IList<ShoppingCartItem>>
{
    /// <summary>
    /// Returns the current inventory count.
    /// </summary>
    Task<int> QueryInventoryAsync(string sku);

    /// <summary>
    /// Updates the inventory of a product based on the provided parameters.
    /// </summary>
    /// <param name="difference">The value to add to or subtract from the inventory.</param>
    /// <param name="reset">Whether inventory count should be reset to zero before adding the difference value.</param>
    void UpdateInventory(ProductPart productPart, int difference, bool reset = false);
}
