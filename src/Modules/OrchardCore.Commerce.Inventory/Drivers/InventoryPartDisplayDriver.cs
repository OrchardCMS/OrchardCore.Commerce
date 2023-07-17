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

    public InventoryPartDisplayDriver(IHttpContextAccessor hca)
    {
        _hca = hca;
    }

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
        // if hca allows retrieving productSku, do the dictionary updating here
        var currentSku = _hca.HttpContext.Request.Form["ProductPart.Sku"].ToString();
        var skuBefore = part.Inventory.FirstOrDefault().Key.Split("-").First();

        var viewModel = new InventoryPartViewModel();
        await updater.TryUpdateModelAsync(viewModel, Prefix);

        //if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        //{
        //    part.Inventory.Clear();
        //    part.Inventory.AddRange(viewModel.Inventory);
        //}

        // update shit if sku changed
        if (currentSku != skuBefore)
        {
            // do the updating of dictionaries -- only inventories? update CanBeBought in ProductPartDisplayDriver?
            var newInventory = new Dictionary<string, int>();
            foreach (var inventoryEntry in part.Inventory)
            {
                var updatedKey = part.Inventory.Count > 1
                    ? currentSku + "-" + inventoryEntry.Key.Split('-').Last()
                    : currentSku;
                newInventory.Add(updatedKey, inventoryEntry.Value);
            }

            part.Inventory.Clear();
            part.Inventory.AddRange(newInventory);
        }
        else
        {


            part.Inventory.Clear();
            part.Inventory.AddRange(viewModel.Inventory);
        }

        //part.Inventory.Clear();
        //part.Inventory.Add(new KeyValuePair<string, int>("SKU", 5));

        return await EditAsync(part, context);
    }

    private static void BuildViewModel(InventoryPartViewModel model, InventoryPart part) =>
        model.Inventory.AddRange(part.Inventory);
}
