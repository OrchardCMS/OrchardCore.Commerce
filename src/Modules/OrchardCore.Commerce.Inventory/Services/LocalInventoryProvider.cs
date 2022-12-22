using OrchardCore.Commerce.Inventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Inventory.Services;

// Problem is this would need to be based on IProductInventoryProvider, but that'd cause circular dependency.
public class LocalInventoryProvider
{
    // Include the actual implementations of the two methods above (QueryInventoryAsync(), UpdateInventoryAsync()).

    // Order property.
    public int Order => 0;

    // Check if product can use this provider.For LocalInventoryProvider this means the presence of the InventoryPart.
    //public async Task<bool> IsApplicableAsync(string sku)
    //{
    //    var productPart = await _productService.GetProductAsync(sku);
    //    return productPart.ContentItem.Has<InventoryPart>();
    //}
}
