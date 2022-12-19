using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Local.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A product inventory service that asks all available product inventory providers to update the inventories of a list
/// of shopping cart items.
/// </summary>
public class ProductInventoryService : IProductInventoryService
{
    private readonly IProductService _productService;
    private readonly IEnumerable<IProductInventoryProvider> _productInventoryProviders;

    public ProductInventoryService(
        IEnumerable<IProductInventoryProvider> productInventoryProviders, IProductService productService)
    {
        _productInventoryProviders = productInventoryProviders;
        _productService = productService;
    }

    public async Task<IList<ShoppingCartItem>> UpdateInventoriesAsync(IList<ShoppingCartItem> items)
    {
        var providers = await _productInventoryProviders
            .OrderBy(provider => provider.Order)
            .WhereAsync(provider => provider.IsApplicableAsync(items));

        foreach (var productInventoryProvider in providers)
        {
            var result = await productInventoryProvider.UpdateAsync(items); // this should be the update inventory call?
            items = result.AsList();
        }

        return items;
    }
}
