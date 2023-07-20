using Fluid.Values;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Inventory.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Inventory.Drivers;

public class InventoryPartDisplayDriver : ContentPartDisplayDriver<InventoryPart>
{
    private readonly IHttpContextAccessor _hca;
    private static readonly SemaphoreSlim _lock = new(initialCount: 1);

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
        var currentSku = _hca.HttpContext.Request.Form["ProductPart.Sku"].ToString().ToUpperInvariant();
        var skuBefore = part.Inventory.FirstOrDefault().Key.Split("-").First();

        var viewModel = new InventoryPartViewModel();
        await updater.TryUpdateModelAsync(viewModel, Prefix, viewModel => viewModel.InventoryValues);

        part.Inventory.RemoveAll();
        viewModel.Inventory.RemoveAll();
        //part.Inventory.AddRange(viewModel.Inventory);

        foreach (var inventoryValue in viewModel.InventoryValues)
        {
            part.Inventory[inventoryValue.Key] = inventoryValue.Value;
        }

        // If SKU was changed, inventory keys need to be updated.
        if (!string.IsNullOrEmpty(currentSku) && currentSku != skuBefore)
        {
            var newInventory = new Dictionary<string, int>();
            var oldInventory = viewModel.InventoryValues.ToDictionary(key => key.Key, value => value.Value);
            foreach (var inventoryEntry in oldInventory)
            {
                var updatedKey = oldInventory.Count > 1
                    ? currentSku + "-" + inventoryEntry.Key.Split('-').Last()
                    : currentSku;

                //part.Inventory.Remove(inventoryEntry.Key);
                //part.Inventory.Add(updatedKey, inventoryEntry.Value);
                var newEntry = new KeyValuePair<string, int>(updatedKey, inventoryEntry.Value);

                //part.Inventory.Add(newEntry);
                newInventory.Add(updatedKey, inventoryEntry.Value);
            }
            //await _lock.WaitAsync();
            //try
            //{

            //}
            //finally { _lock.Release(); }

            part.Inventory.Clear();
            part.Inventory.AddRange(newInventory);
        }

        part.ProductSku = currentSku;

        return await EditAsync(part, context);
    }

    private static void BuildViewModel(InventoryPartViewModel model, InventoryPart part)
    {
        var inventory = part.Inventory ?? new Dictionary<string, int>();

        // Workaround for InventoryPart storing the outdated inventory entries along with the updated ones.
        var values = inventory
            .Where(kvp => kvp.Key.Contains(part.ProductSku))
            .ToDictionary(key => key.Key, value => value.Value);

        model.InitializeInventory(inventory, values);
    }
}
