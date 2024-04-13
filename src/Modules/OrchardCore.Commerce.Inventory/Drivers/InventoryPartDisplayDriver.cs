using GraphQL.Introspection;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Inventory.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
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
        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            var currentSku = _hca.HttpContext?.Request.Form["ProductPart.Sku"].ToString().ToUpperInvariant();
            var skuBefore = viewModel.Inventory.FirstOrDefault().Key != null
                ? viewModel.Inventory.FirstOrDefault().Key.Split('-')[0]
                : currentSku;

            part.Inventory.Clear();
            part.Inventory.AddRange(viewModel.Inventory);

            // If SKU was changed, inventory keys need to be updated.
            if (!string.IsNullOrEmpty(currentSku) && currentSku != skuBefore)
            {
                var newInventory = part.Inventory.ToDictionary(
                    item => item.Key.Partition("-").Right is { } suffix ? $"{currentSku}-{suffix}" : currentSku,
                    item => item.Value);

                part.Inventory.SetItems(newInventory);
                part.InventoryKeys.SetItems(newInventory.Keys);
            }

            part.ProductSku = currentSku;
        }

        return await EditAsync(part, context);
    }

    // Despite the Clear() calls inside UpdateAsync(), the Inventory property retains its old values along with the
    // new ones, hence the filtering below.
    private static void BuildViewModel(InventoryPartViewModel model, InventoryPart part) =>
        model.Inventory.SetItems(part.FilterOutdatedEntries());
}
