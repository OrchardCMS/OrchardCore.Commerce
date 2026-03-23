using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Inventory;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class ProductPartDisplayDriver : ContentPartDisplayDriver<ProductPart>
{
    private readonly IProductAttributeService _productAttributeService;
    private readonly ISkuGenerator _skuGenerator;
    private readonly IStringLocalizer T;

    private bool IsSkuReadOnly => _skuGenerator?.IsManualAllowed == false;

    public ProductPartDisplayDriver(
        IProductAttributeService productAttributeService,
        IEnumerable<ISkuGenerator> skuGenerators,
        IStringLocalizer<ProductPartDisplayDriver> stringLocalizer)
    {
        _productAttributeService = productAttributeService;
        _skuGenerator = skuGenerators.HighestPriority();
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
        UpdatePartEditorContext context)
    {
        var skuBefore = part.Sku ?? string.Empty;

        await context.Updater.TryUpdateModelAsync(part, Prefix);

        part.Sku ??= string.Empty;
        if (part.Sku.Contains('-'))
        {
            context.AddModelError(nameof(ProductPart.Sku), T["SKU may not contain the dash character."]);
            return await EditAsync(part, context);
        }

        part.Sku = part.Sku.ToUpperInvariant();

        // If the SKU is read-only then editing should not be possible, but here we undo any POST trickery just in case.
        if (IsSkuReadOnly)
        {
            part.Sku = skuBefore;
        }

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
        viewModel.IsSkuReadOnly = IsSkuReadOnly;
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
