using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
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

        // If the SKU is read-only then editing should not be possible, but here we undo any POST trickery just in case.
        part.Sku = IsSkuReadOnly ? skuBefore : part.Sku.ToUpperInvariant();

        _productAttributeService.UpdateCanBeBoughtForProductAttributeField(part, skuBefore);

        return await EditAsync(part, context);
    }

    private async Task BuildViewModelAsync(ProductPartViewModel viewModel, ProductPart part)
    {
        viewModel.ContentItem = part.ContentItem;
        viewModel.Sku = part.Sku;
        viewModel.IsSkuReadOnly = IsSkuReadOnly;
        viewModel.ProductPart = part;

        _productAttributeService.UpdateCanBeBoughtForProductAttributeField(part, part.Sku);
        viewModel.CanBeBought.SetItems(part.CanBeBought);
        viewModel.Attributes = await _productAttributeService.GetProductAttributeFieldsAsync(part.ContentItem);
    }
}
