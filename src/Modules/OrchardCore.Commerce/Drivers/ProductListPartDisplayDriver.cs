using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Lombiq.HelpfulLibraries.OrchardCore.Contents.CommonContentDisplayTypes;

namespace OrchardCore.Commerce.Drivers;

public class ProductListPartDisplayDriver : ContentPartDisplayDriver<ProductListPart>
{
    private readonly IProductListService _productListService;
    private readonly IEnumerable<IProductFilterParametersProvider> _productFilterProviders;

    public ProductListPartDisplayDriver(
        IProductListService productListService,
        IEnumerable<IProductFilterParametersProvider> productFilterProviders)
    {
        _productListService = productListService;
        _productFilterProviders = productFilterProviders;
    }

    public override IDisplayResult Display(ProductListPart part, BuildPartDisplayContext context) =>
        Combine(
            Initialize<ProductListPartViewModel>(
                    GetDisplayShapeType(context),
                    async viewModel => await BuildViewModelAsync(viewModel, part, context))
                .Location(Detail, "Content:25")
                .Location(Summary, "Meta:10"),
            Initialize<ProductListOrderByViewModel>(
                    "ProductList_OrderBy",
                    async viewModel => await BuildOrderByViewModelAsync(viewModel, part))
                .Location(Detail, "Content:20"));

    private async Task BuildViewModelAsync(ProductListPartViewModel viewModel, ProductListPart part, BuildPartDisplayContext context)
    {
        viewModel.ProductListPart = part;

        var filterParameters = await _productFilterProviders
            .MaxBy(provider => provider.Priority).GetFilterParametersAsync(part) ?? new ProductListFilterParameters();

        var productList = await _productListService.GetProductsAsync(part, filterParameters);
        viewModel.Products = productList.Products;
        viewModel.Pager = (await context.New.Pager(filterParameters.Pager)).TotalItemCount(productList.TotalItemCount);
        viewModel.Context = context;
    }

    private async Task BuildOrderByViewModelAsync(ProductListOrderByViewModel viewModel, ProductListPart part)
    {
        viewModel.ProductListPart = part;

        var options = await _productListService.GetOrderByOptionsAsync(part);
        viewModel.OrderByOptions = options;
    }
}
