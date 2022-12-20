using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Inventory.Local.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A product inventory provider that updates inventories of shopping cart items which have an <c>InventoryPart</c>.
/// </summary>
public class ProductInventoryProvider : IProductInventoryProvider
{
    private readonly IProductService _productService;
    private readonly ISession _session;

    public ProductInventoryProvider(IProductService productService, ISession session)
    {
        _productService = productService;
        _session = session;
    }

    public int Order => 0;

    public async Task<bool> IsApplicableAsync(IList<ShoppingCartItem> model)
    {
        var skuProducts = await _productService.GetSkuProductsAsync(model);

        return model.All(item =>
            skuProducts.TryGetValue(item.ProductSku, out var productPart) &&
            productPart.ContentItem.Has<InventoryPart>());
    }

    public async Task<int> QueryInventoryAsync(string sku)
    {
        var inventoryPart = (await _productService.GetProductAsync(sku))?.As<InventoryPart>();
        return inventoryPart != null ? (int)inventoryPart.Inventory.Value : 0;
    }

    public async Task<IList<ShoppingCartItem>> UpdateAsync(IList<ShoppingCartItem> model)
    {
        foreach (var item in model)
        {
            UpdateInventory(await _productService.GetProductAsync(item.ProductSku), item.Quantity);
        }

        return model;
    }

    public void UpdateInventory(ProductPart productPart, int difference, bool reset = false)
    {
        var inventoryPart = productPart.As<InventoryPart>();
        if (inventoryPart == null || inventoryPart.IgnoreInventory.Value) return;

        var newValue = reset ? difference : ((int)inventoryPart.Inventory.Value) + difference;

        inventoryPart.Inventory.Value = newValue > 0 ? newValue : 0;
        inventoryPart.Apply();

        _session.Save(productPart.ContentItem);
    }
}
