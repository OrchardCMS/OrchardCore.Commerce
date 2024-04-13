using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory;
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
        Initialize<ProductPartViewModel>(GetDisplayShapeType(context), async viewModel => await BuildViewModelAsync(viewModel, part))
            .Location("Detail", "Content:20")
            .Location("Summary", "Meta:5");

    public override IDisplayResult Edit(ProductPart part, BuildPartEditorContext context) =>
        Initialize<ProductPartViewModel>(GetEditorShapeType(context), async viewModel => await BuildViewModelAsync(viewModel, part));

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

        if (part.ContentItem.As<InventoryPart>() is { } inventoryPart)
        {
            part.CanBeBought.Clear();

            var filteredInventory = inventoryPart.FilterOutdatedEntries();

            // If an inventory's value is below 1 and back ordering is not allowed, corresponding
            // CanBeBought entry needs to be set to false; should be set to true otherwise.
            foreach (var inventory in filteredInventory)
            {
                part.CanBeBought[inventory.Key] = inventoryPart.AllowsBackOrder.Value || inventory.Value >= 1;
            }

            // If SKU was updated, CanBeBought keys also need to be updated.
            if (part.Sku != skuBefore)
            {
                UpdateAvailabilityKeys(part, filteredInventory.Count);
            }
        }

        return await EditAsync(part, context);
    }

    private static void UpdateAvailabilityKeys(ProductPart part, int inventoryCount)
    {
        var newAvailabilities = new Dictionary<string, bool>();
        foreach (var entry in part.CanBeBought)
        {
            var updatedKey = inventoryCount > 1
                ? $"{part.Sku}-{entry.Key.Split('-')[^1]}"
                : part.Sku;

            newAvailabilities.Add(updatedKey, entry.Value);
        }

        part.CanBeBought.Clear();
        part.CanBeBought.AddRange(newAvailabilities);
    }

    private async Task BuildViewModelAsync(ProductPartViewModel viewModel, ProductPart part)
    {
        viewModel.ContentItem = part.ContentItem;
        viewModel.Sku = part.Sku;
        viewModel.ProductPart = part;

        if (part.ContentItem.As<InventoryPart>() is { } inventoryPart)
        {
            foreach (var (key, value) in inventoryPart.FilterOutdatedEntries())
            {
                // If an inventory's value is below 1 and back ordering is not allowed, corresponding
                // CanBeBought entry needs to be set to false; should be set to true otherwise.
                viewModel.CanBeBought[key] = inventoryPart.AllowsBackOrder.Value || value >= 1;
            }
        }
        else
        {
            viewModel.CanBeBought[part.ContentItem.ContentItemId] = true;
        }

        viewModel.Attributes = await _productAttributeService.GetProductAttributeFieldsAsync(part.ContentItem);
    }
}
