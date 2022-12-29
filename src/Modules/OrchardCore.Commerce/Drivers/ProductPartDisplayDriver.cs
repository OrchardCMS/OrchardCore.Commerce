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
        await updater.TryUpdateModelAsync(part, Prefix);

        if (part.Sku.Contains('-'))
        {
            updater.ModelState.AddModelError(nameof(ProductPart.Sku), T["SKU may not contain the dash character."]);
            return await EditAsync(part, context);
        }

        part.Sku = part.Sku.ToUpperInvariant();

        return await EditAsync(part, context);
    }

    private void BuildViewModel(ProductPartViewModel model, ProductPart part)
    {
        model.ContentItem = part.ContentItem;
        model.Sku = part.Sku;
        model.ProductPart = part;

        if (part.As<InventoryPart>() is { } inventoryPart &&
            !inventoryPart.AllowsBackOrder.Value &&
            inventoryPart.Inventory.Value < 1)
        {
            model.CanBeBought = false;
        }

        model.Attributes = _productAttributeService.GetProductAttributeFields(part.ContentItem);
    }
}
