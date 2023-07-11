using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class InventoryShoppingCartEvents : ShoppingCartEventsBase
{
    private readonly IProductService _productService;
    private readonly IHtmlLocalizer<InventoryShoppingCartEvents> H;

    public InventoryShoppingCartEvents(
        IProductService productService,
        IHtmlLocalizer<InventoryShoppingCartEvents> localizer)
    {
        _productService = productService;
        H = localizer;
    }

    public override int Order => 0;

    public override async Task<LocalizedHtmlString> VerifyingItemAsync(ShoppingCartItem item)
    {
        // If the product doesn't have InventoryPart then this event is not applicable.
            // also add Price Variants Product?
        if (await _productService.GetProductAsync(item.ProductSku) is not { } product ||
            product.As<InventoryPart>() is not { } inventoryPart)
        {
            return null;
        }

        var title = product.ContentItem.DisplayText;

        // check the inventory of the item in question
            // using ProductSku property?

        // Item verification should fail if back ordering is not allowed and quantity exceeds available inventory.
        if (!inventoryPart.AllowsBackOrder.Value && item.Quantity > inventoryPart.Inventory.Value)
        {
            return H["There aren't enough {0} left in stock.", title];
        }

        // Item verification should fail if max quantity is set and quantity exceeds its value.
        var checkMaxQuantity = inventoryPart.MaximumOrderQuantity.Value != 0;
        if (checkMaxQuantity && item.Quantity > inventoryPart.MaximumOrderQuantity.Value)
        {
            return H["The checkout quantity for {0} is more than the maximum allowed.", title];
        }

        // Item verification should fail if min quantity is set and quantity is below its value.
        var checkMinQuantity = inventoryPart.MinimumOrderQuantity.Value != 0;
        if (checkMinQuantity && item.Quantity < inventoryPart.MinimumOrderQuantity.Value)
        {
            return H["The checkout quantity for {0} is less than the minimum allowed.", title];
        }

        return null;
    }
}
