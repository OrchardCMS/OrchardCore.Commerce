using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class ProductPartDisplayDriver : ContentPartDisplayDriver<ProductPart>
{
    private readonly IProductAttributeService _productAttributeService;

    public ProductPartDisplayDriver(IProductAttributeService productAttributeService) =>
        _productAttributeService = productAttributeService;

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
        await updater.TryUpdateModelAsync(part, Prefix, productPart => productPart.Sku);

        return await EditAsync(part, context);
    }

    private void BuildViewModel(ProductPartViewModel model, ProductPart part)
    {
        model.ContentItem = part.ContentItem;
        model.Sku = part.Sku;
        model.ProductPart = part;

        model.Attributes = _productAttributeService.GetProductAttributeFields(part.ContentItem);
    }
}
