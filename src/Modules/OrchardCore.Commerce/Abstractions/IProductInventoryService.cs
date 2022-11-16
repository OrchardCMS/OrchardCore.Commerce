using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Contains inventory management related methods.
/// </summary>
public interface IProductInventoryService
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
    Task UpdateInventoryAsync(string sku, int difference, bool reset = false);
}
