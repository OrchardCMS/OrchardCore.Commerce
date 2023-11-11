using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class InventoryCheckoutEvents : ICheckoutEvents
{
    private readonly IProductService _productService;

    public InventoryCheckoutEvents(IProductService productService) => _productService = productService;

    public Task<bool> ViewModelCreatedAsync(IList<ShoppingCartLineViewModel> lines)
    {
        var cannotCheckout = false;
        foreach (var line in lines)
        {
            var productPart = line.Product.ContentItem.As<ProductPart>();
            if (productPart.As<InventoryPart>() is not { } inventoryPart)
            {
                continue;
            }

            var item = new ShoppingCartItem(line.Quantity, line.ProductSku, line.Attributes?.Values);
            var fullSku = _productService.GetOrderFullSku(item, productPart);
            var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? productPart.Sku : fullSku;
            var relevantInventory = inventoryPart.Inventory.FirstOrDefault(entry => entry.Key == inventoryIdentifier);

            cannotCheckout = relevantInventory.Value < 1 &&
                !inventoryPart.AllowsBackOrder.Value &&
                !inventoryPart.IgnoreInventory.Value;

            if (cannotCheckout)
            {
                return Task.FromResult(cannotCheckout);
            }
        }

        return Task.FromResult(cannotCheckout);
    }
}
