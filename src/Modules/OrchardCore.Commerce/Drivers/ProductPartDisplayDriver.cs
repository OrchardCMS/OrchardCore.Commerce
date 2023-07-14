using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class ProductPartDisplayDriver : ContentPartDisplayDriver<ProductPart>
{
    private readonly IProductAttributeService _productAttributeService;
    private readonly IStringLocalizer T;

    public ProductPartDisplayDriver(
        IProductAttributeService productAttributeService,
        IStringLocalizer<ProductPartDisplayDriver> stringLocalizer)
    {
        _productAttributeService = productAttributeService;
        T = stringLocalizer;
    }

    public override IDisplayResult Display(ProductPart part, BuildPartDisplayContext context) =>
        Initialize<ProductPartViewModel>(GetDisplayShapeType(context), viewModel => BuildViewModel(viewModel, part))
            .Location("Detail", "Content:20")
            .Location("Summary", "Meta:5");

    public override IDisplayResult Edit(ProductPart part, BuildPartEditorContext context) =>
        Initialize<ProductPartViewModel>(GetEditorShapeType(context), viewModel => BuildViewModel(viewModel, part));

    public override async Task<IDisplayResult> UpdateAsync(
        ProductPart part,
        IUpdateModel updater,
        UpdatePartEditorContext context)
    {
        var skuBefore = part.Sku;

        await updater.TryUpdateModelAsync(part, Prefix);

        if (part.Sku.Contains('-'))
        {
            updater.ModelState.AddModelError(nameof(ProductPart.Sku), T["SKU may not contain the dash character."]);
            return await EditAsync(part, context);
        }

        part.Sku = part.Sku.ToUpperInvariant();

        // If SKU was updated, inventory keys also need to be updated.
        if (part.Sku != skuBefore && part.As<InventoryPart>() is { } inventoryPart)
        {
            // No need if we are dealing with a Product. To be edited later to include regular Products too.
            if (inventoryPart.Inventory.Count < 1) return await EditAsync(part, context);

            var newInventory = new Dictionary<string, int>();
            foreach (var inventoryEntry in inventoryPart.Inventory)
            {
                var updatedKey = part.Sku + "-" + inventoryEntry.Key.Split('-').Last();
                newInventory.Add(updatedKey, inventoryEntry.Value);
            }

            inventoryPart.Inventory.Clear();
            inventoryPart.Inventory.AddRange(newInventory);
        }

        return await EditAsync(part, context);
    }

    private void BuildViewModel(ProductPartViewModel viewModel, ProductPart part)
    {
        viewModel.ContentItem = part.ContentItem;
        viewModel.Sku = part.Sku;
        viewModel.ProductPart = part;

        if (part.As<InventoryPart>() is { } inventoryPart)
        {
            foreach (var inventory in inventoryPart.Inventory)
            {
                // If an inventory's value is below 1 and back ordering is not allowed, corresponding
                // CanBeBought entry needs to be set to false; should be set to true otherwise.
                viewModel.CanBeBought[inventory.Key] = inventoryPart.AllowsBackOrder.Value || inventory.Value >= 1;
            }
        }

        viewModel.Attributes = _productAttributeService.GetProductAttributeFields(part.ContentItem);
    }
}
