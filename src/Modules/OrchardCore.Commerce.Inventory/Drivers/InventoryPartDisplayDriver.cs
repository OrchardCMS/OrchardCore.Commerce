using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Inventory.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Inventory.Drivers;

public sealed class InventoryPartDisplayDriver : ContentPartDisplayDriver<InventoryPart>
{
    public const string NewProductKey = "DEFAULT";

    private readonly IHttpContextAccessor _hca;

    internal readonly IStringLocalizer T;

    public InventoryPartDisplayDriver(IHttpContextAccessor hca, IStringLocalizer<InventoryPartDisplayDriver> localizer)
    {
        _hca = hca;
        T = localizer;
    }

    public override IDisplayResult Display(InventoryPart part, BuildPartDisplayContext context) =>
        Initialize<InventoryPartViewModel>(GetDisplayShapeType(context), viewModel => BuildViewModel(viewModel, part))
            .Location("Detail", "Content:26")
            .Location("Summary", "Meta");

    public override IDisplayResult Edit(InventoryPart part, BuildPartEditorContext context) =>
        Initialize<InventoryPartViewModel>(GetEditorShapeType(context), viewModel => BuildViewModel(viewModel, part));

    public override async Task<IDisplayResult> UpdateAsync(InventoryPart part, UpdatePartEditorContext context)
    {
        if (_hca.HttpContext?.Request.Form["ProductPart.Sku"].ToString().ToUpperInvariant() is not { } currentSku)
        {
            context.AddModelError("ProductPart.Sku", T["The Product SKU is missing."]);
            return await EditAsync(part, context);
        }

        var viewModel = await context.CreateModelAsync<InventoryPartViewModel>(Prefix);
        var inventory = viewModel.Inventory;

        // Workaround for accepting inventory values during content item creation where the SKU is not yet known.
        if (inventory.TryGetValue(NewProductKey, out var defaultCount))
        {
            inventory.Remove(NewProductKey);
            inventory.Add(currentSku, defaultCount);
        }

        var skuBefore = inventory.FirstOrDefault().Key.Split('-')[0];

        part.Inventory.SetItems(inventory);

        var skuChanged = !string.IsNullOrEmpty(currentSku) && (context.IsNew || currentSku != skuBefore);
        if (skuChanged && part.Inventory.Count == 1 && !part.Inventory.Keys.Single().Contains('-'))
        {
            part.Inventory.SetItems([new(currentSku, part.Inventory.Values.Single())]);
            part.InventoryKeys.SetItems([currentSku]);
        }
        else if (skuChanged)
        {
            var newInventory = part.Inventory.ToDictionary(
                item => $"{currentSku}-{item.Key.Split('-', 2)[^1]}",
                item => item.Value);

            part.Inventory.SetItems(newInventory);
            part.InventoryKeys.SetItems(newInventory.Keys);
        }

        part.ProductSku = currentSku;

        return await EditAsync(part, context);
    }

    // Despite the Clear() calls inside UpdateAsync(), the Inventory property retains its old values along with the
    // new ones, hence the filtering below.
    private static void BuildViewModel(InventoryPartViewModel model, InventoryPart part)
    {
        model.Inventory.SetItems(part.FilterOutdatedEntries());

        var sku = part.ContentItem?.Content.ProductPart?.Sku?.ToString() as string;
        if (model.Inventory.Count == 0) model.Inventory.Add(sku ?? NewProductKey, 0);
    }
}
