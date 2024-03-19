using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
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
    private readonly IProductInventoryService _productInventoryService;

    public InventoryShoppingCartEvents(
        IProductService productService,
        IHtmlLocalizer<InventoryShoppingCartEvents> localizer,
        IProductInventoryService productInventoryService)
    {
        _productService = productService;
        _productInventoryService = productInventoryService;
        H = localizer;
    }

    public override int Order => 0;

    public override async Task ViewModelCreatedAsync(ShoppingCartViewModel viewModel)
    {
        if (await _productInventoryService.VerifyLinesAsync(viewModel.Lines))
        {
            viewModel.InvalidReasons.Add(H["An item in cart is out of stock."]);
        }
    }

    public override async Task<LocalizedHtmlString> VerifyingItemAsync(ShoppingCartItem item)
    {
        // If the product doesn't have InventoryPart then this event is not applicable.
        if (await _productService.GetProductAsync(item.ProductSku) is not { } productPart ||
            productPart.ContentItem.As<InventoryPart>() is not { } inventoryPart)
        {
            return null;
        }

        // If IgnoreInventory is set to true, inventory checks are unnecessary.
        if (inventoryPart.IgnoreInventory.Value)
        {
            return null;
        }

        // If there are no attributes on a Price Variant Product, there's no need for the below checks.
        if (productPart.ContentItem.As<PriceVariantsPart>() is not null && !item.Attributes.Any())
        {
            return null;
        }

        var title = productPart.ContentItem.DisplayText;
        var fullSku = await _productService.GetOrderFullSkuAsync(item, productPart);

        var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? productPart.Sku : fullSku;
        var relevantInventory = inventoryPart.Inventory.FirstOrDefault(entry => entry.Key == inventoryIdentifier);

        // Item verification should fail if back ordering is not allowed and quantity exceeds available inventory.
        if (!inventoryPart.AllowsBackOrder.Value && item.Quantity > relevantInventory.Value)
        {
            return H["There are not enough {0} left in stock.", title];
        }

        // Item verification should fail if max order quantity is set and quantity exceeds its value.
        var maxOrderQuantity = inventoryPart.MaximumOrderQuantity.Value;
        var checkMaxQuantity = maxOrderQuantity != 0;
        if (checkMaxQuantity && item.Quantity > maxOrderQuantity)
        {
            return H["The checkout quantity for {0} is more than the maximum allowed ({1}).", title, maxOrderQuantity];
        }

        // Item verification should fail if min order quantity is set and quantity is below its value.
        var minOrderQuantity = inventoryPart.MinimumOrderQuantity.Value;
        var checkMinQuantity = minOrderQuantity != 0;
        if (checkMinQuantity && item.Quantity < minOrderQuantity)
        {
            return H["The checkout quantity for {0} is less than the minimum allowed ({1}).", title, minOrderQuantity];
        }

        return null;
    }
}
