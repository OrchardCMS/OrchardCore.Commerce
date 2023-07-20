using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Inventory.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Inventory.Drivers;

public class InventoryPartDisplayDriver : ContentPartDisplayDriver<InventoryPart>
{
    private readonly IHttpContextAccessor _hca;

    public InventoryPartDisplayDriver(IHttpContextAccessor hca) => _hca = hca;

    public override IDisplayResult Display(InventoryPart part, BuildPartDisplayContext context) =>
        Initialize<InventoryPartViewModel>(GetDisplayShapeType(context), viewModel => BuildViewModel(viewModel, part))
            .Location("Detail", "Content:26")
            .Location("Summary", "Meta");

    public override IDisplayResult Edit(InventoryPart part, BuildPartEditorContext context) =>
        Initialize<InventoryPartViewModel>(GetEditorShapeType(context), viewModel => BuildViewModel(viewModel, part));

    public override async Task<IDisplayResult> UpdateAsync(
        InventoryPart part,
        IUpdateModel updater,
        UpdatePartEditorContext context)
    {
        var viewModel = new InventoryPartViewModel();
        await updater.TryUpdateModelAsync(viewModel, Prefix);

        var currentSku = _hca.HttpContext.Request.Form["ProductPart.Sku"].ToString().ToUpperInvariant();
        var skuBefore = viewModel.Inventory.FirstOrDefault().Key != null
            ? viewModel.Inventory.FirstOrDefault().Key.Split("-").First()
            : "DEFAULT";

        part.Inventory.Clear();
        part.Inventory.AddRange(viewModel.Inventory);

        // If SKU was changed, inventory keys need to be updated.
        if (!string.IsNullOrEmpty(currentSku) && currentSku != skuBefore)
        {
            var newInventory = new Dictionary<string, int>();
            var oldInventory = part.Inventory.ToDictionary(key => key.Key, value => value.Value);
            foreach (var inventoryEntry in oldInventory)
            {
                var updatedKey = oldInventory.Count > 1
                    ? currentSku + "-" + inventoryEntry.Key.Split('-').Last()
                    : currentSku;

                part.Inventory.Remove(inventoryEntry.Key);
                newInventory.Add(updatedKey, inventoryEntry.Value);
            }

            part.Inventory.Clear();
            part.Inventory.AddRange(newInventory);
        }

        part.ProductSku = currentSku;

        return await EditAsync(part, context);
    }

    // Despite the Clear() calls inside UpdateAsync(), the Inventory property retains its old values along with the
    // new ones, hence the filtering below.
    private static void BuildViewModel(InventoryPartViewModel model, InventoryPart part)
    {
        var inventory = part.Inventory ?? new Dictionary<string, int>();
        if (inventory.Any())
        {
            // Workaround for InventoryPart storing the outdated inventory entries along with the updated ones.
            var filteredInventory = inventory
                .Where(kvp => kvp.Key.Contains(part.ProductSku))
                .ToDictionary(key => key.Key, value => value.Value);

            model.Inventory.AddRange(filteredInventory);
        }
        else
        {
            // When creating a new item, initialize a default inventory.
            model.Inventory.Add("DEFAULT", 0);
        }
    }
}
