using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.Inventory.Models;
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
    private readonly IEnumerable<IProductInventoryProvider> _productInventoryProviders;
    private readonly IProductService _productService;

    public ProductInventoryService(IEnumerable<IProductInventoryProvider> productInventoryProviders, IProductService productService)
    {
        _productInventoryProviders = productInventoryProviders;
        _productService = productService;
    }

    public async Task<IList<ShoppingCartItem>> UpdateInventoriesAsync(IList<ShoppingCartItem> items)
    {
        if (await _productInventoryProviders.GetFirstApplicableProviderAsync(items) is { } provider)
        {
            await provider.UpdateAsync(items);
        }

        return items;
    }

    public async Task<bool> VerifyLinesAsync(IList<ShoppingCartLineViewModel> lines)
    {
        foreach (var line in lines)
        {
            var cannotCheckout = false;
            var productPart = line.Product.ContentItem.As<ProductPart>();
            if (productPart.As<InventoryPart>() is not { } inventoryPart)
            {
                continue;
            }

            var item = new ShoppingCartItem(line.Quantity, line.ProductSku, line.Attributes?.Values);
            var fullSku = await _productService.GetOrderFullSkuAsync(item, productPart);
            var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? productPart.Sku : fullSku;
            var relevantInventory = inventoryPart.Inventory.TryGetValue(inventoryIdentifier, out var inventory)
                ? inventory
                : inventoryPart.Inventory.GetMaybe(productPart.Sku);

            cannotCheckout = relevantInventory < 1 &&
                !inventoryPart.AllowsBackOrder.Value &&
                !inventoryPart.IgnoreInventory.Value;

            if (cannotCheckout)
            {
                return true;
            }
        }

        return false;
    }
}
