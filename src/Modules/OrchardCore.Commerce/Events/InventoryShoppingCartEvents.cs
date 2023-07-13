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
    private readonly IPredefinedValuesProductAttributeService _predefinedValuesService;

    public InventoryShoppingCartEvents(
        IProductService productService,
        IHtmlLocalizer<InventoryShoppingCartEvents> localizer,
        IPredefinedValuesProductAttributeService predefinedValuesService)
    {
        _productService = productService;
        H = localizer;
        _predefinedValuesService = predefinedValuesService;
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

        var attributesRestrictedToPredefinedValues = _predefinedValuesService
            .GetProductAttributesRestrictedToPredefinedValues(productPart.ContentItem)
            .Select(attr => attr.PartName + "." + attr.Name)
            .ToHashSet();

        var variantKey = item.GetVariantKeyFromAttributes(attributesRestrictedToPredefinedValues);
        var fullSku = item.Attributes.Any()
            ? item.ProductSku + "-" + variantKey
            : string.Empty;

        var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? "DEFAULT" : fullSku.ToUpperInvariant();
        var relevantInventory = inventoryPart.Inventoree.FirstOrDefault(entry => entry.Key == inventoryIdentifier);

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
