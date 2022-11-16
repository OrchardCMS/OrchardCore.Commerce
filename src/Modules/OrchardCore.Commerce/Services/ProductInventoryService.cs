using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Local.Models;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class ProductInventoryService : IProductInventoryService
{
    private readonly IProductService _productService;

    public ProductInventoryService(IProductService productService) => _productService = productService;

    public async Task<int> QueryInventoryAsync(string sku)
    {
        var inventoryPart = (await _productService.GetProductAsync(sku))?.As<InventoryPart>();
        return inventoryPart != null ? (int)inventoryPart.Inventory.Value : 0;
    }

    public async Task UpdateInventoryAsync(string sku, int difference, bool reset = false)
    {
        var inventoryPart = (await _productService.GetProductAsync(sku))?.As<InventoryPart>();
        if (inventoryPart == null) return;

        var newValue = reset ? difference : ((int)inventoryPart.Inventory.Value) + difference;

        inventoryPart.Inventory.Value = newValue;
        inventoryPart.Apply();
    }
}
