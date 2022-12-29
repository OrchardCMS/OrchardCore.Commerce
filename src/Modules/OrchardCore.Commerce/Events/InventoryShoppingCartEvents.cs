using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class InventoryShoppingCartEvents : ShoppingCartEventsBase
{
    private readonly IProductService _productService;

    public InventoryShoppingCartEvents(IProductService productService) => _productService = productService;

    public override int Order => 0;

    public override async Task<bool> VerifyingItemAsync(ShoppingCartItem item)
    {
        // If the product doesn't have InventoryPart then this event is not applicable.
        if ((await _productService.GetProductAsync(item.ProductSku)).As<InventoryPart>() is not { } inventoryPart)
        {
            return true;
        }

        // Item verification should fail if back ordering is not allowed and quantity exceeds available inventory.
        if (!inventoryPart.AllowsBackOrder.Value && item.Quantity > inventoryPart.Inventory.Value)
        {
            return false;
        }

        // Item verification should fail if max quantity is set and quantity exceeds its value.
        var checkMaxQuantity = inventoryPart.MaximumOrderQuantity.Value != 0;
        if (checkMaxQuantity && item.Quantity > inventoryPart.MaximumOrderQuantity.Value)
        {
            return false;
        }

        // Item verification should fail if min quantity is set and quantity is below its value.
        var checkMinQuantity = inventoryPart.MinimumOrderQuantity.Value != 0;
        if (checkMinQuantity && item.Quantity < inventoryPart.MinimumOrderQuantity.Value)
        {
            return false;
        }

        return true;
    }
}
