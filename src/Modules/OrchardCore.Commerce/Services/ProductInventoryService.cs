using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A product inventory service that asks all available product inventory providers to update the inventories of a list
/// of shopping cart items.
/// </summary>
public class ProductInventoryService : IProductInventoryService
{
    private readonly IEnumerable<IProductInventoryProvider> _productInventoryProviders;

    public ProductInventoryService(IEnumerable<IProductInventoryProvider> productInventoryProviders) =>
        _productInventoryProviders = productInventoryProviders;

    public async Task<IList<ShoppingCartItem>> UpdateInventoriesAsync(IList<ShoppingCartItem> items)
    {
        if (await _productInventoryProviders.GetFirstApplicableProviderAsync(items) is { } provider)
        {
            await provider.UpdateAsync(items);
        }

        return items;
    }
}
