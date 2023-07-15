using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Linq;
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
        if (await _productService.GetProductAsync(item.ProductSku) is not { } productPart ||
            productPart.As<InventoryPart>() is not { } inventoryPart)
        {
            return null;
        }

        // If there are no attributes on a Price Variant Product, there's no need for the below checks.
        if (productPart.As<PriceVariantsPart>() is not null && !item.Attributes.Any())
        {
            return null;
        }

        var title = productPart.ContentItem.DisplayText;
        var fullSku = _productService.GetOrderFullSku(item, productPart);

        var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? productPart.Sku : fullSku;
        var relevantInventory = inventoryPart.Inventory.FirstOrDefault(entry => entry.Key == inventoryIdentifier);

        // Item verification should fail if back ordering is not allowed and quantity exceeds available inventory.
        if (!inventoryPart.AllowsBackOrder.Value && item.Quantity > relevantInventory.Value)
        {
            return H["There aren't enough {0} left in stock.", title];
        }

        // Item verification should fail if max order quantity is set and quantity exceeds its value.
        var checkMaxQuantity = inventoryPart.MaximumOrderQuantity.Value != 0; // tbd quantities?
        if (checkMaxQuantity && item.Quantity > inventoryPart.MaximumOrderQuantity.Value)
        {
            return H["The checkout quantity for {0} is more than the maximum allowed.", title];
        }

        // Item verification should fail if min order quantity is set and quantity is below its value.
        var checkMinQuantity = inventoryPart.MinimumOrderQuantity.Value != 0; // tbd quantities?
        if (checkMinQuantity && item.Quantity < inventoryPart.MinimumOrderQuantity.Value)
        {
            return H["The checkout quantity for {0} is less than the minimum allowed.", title];
        }

        return null;
    }
}
