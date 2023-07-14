using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Inventory.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Inventory.Drivers;

public class InventoryPartDisplayDriver : ContentPartDisplayDriver<InventoryPart>
{
    public override IDisplayResult Display(InventoryPart part, BuildPartDisplayContext context) =>
        Initialize<InventoryPartViewModel>(GetDisplayShapeType(context), viewModel => BuildViewModel(viewModel, part))
            .Location("Detail", "Content:26")
            .Location("Summary", "Meta:6");

    public override IDisplayResult Edit(InventoryPart part, BuildPartEditorContext context) =>
        Initialize<InventoryPartViewModel>(GetEditorShapeType(context), viewModel =>
        {
            BuildViewModel(viewModel, part);
        });

    public override async Task<IDisplayResult> UpdateAsync(
        InventoryPart part,
        IUpdateModel updater,
        UpdatePartEditorContext context)
    {
        var viewModel = new InventoryPartViewModel();
        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            part.Inventory.Clear();
            part.Inventory.AddRange(viewModel.Inventory);

            //context.Updater.ModelState.
        }

        return await EditAsync(part, context);
    }

    private static void BuildViewModel(InventoryPartViewModel model, InventoryPart part)
    {
        model.Inventory.AddRange(part.Inventory);
    }
}
