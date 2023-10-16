using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using System.Threading.Tasks;
using static Lombiq.HelpfulLibraries.OrchardCore.Contents.CommonContentDisplayTypes;

namespace OrchardCore.Commerce.Drivers;

public class ProductListPartDisplayDriver : ContentPartDisplayDriver<ProductListPart>
{
    private readonly ISiteService _siteService;
    private readonly IProductListService _productListService;

    public ProductListPartDisplayDriver(ISiteService siteService, IProductListService productListService)
    {
        _siteService = siteService;
        _productListService = productListService;
    }

    public override IDisplayResult Display(ProductListPart part, BuildPartDisplayContext context) =>
        Initialize<ProductListPartViewModel>(GetDisplayShapeType(context), async viewModel => await BuildViewModelAsync(viewModel, part, context))
            .Location(Detail, "Content:25")
            .Location(Summary, "Meta:10");

    private async Task BuildViewModelAsync(ProductListPartViewModel viewModel, ProductListPart part, BuildPartDisplayContext context)
    {
        viewModel.ProductListPart = part;

        var pager = await GetPagerAsync(context);
        viewModel.Pager = await context.New.Pager(pager);
        viewModel.Products = await _productListService.GetProductsAsync(part, pager);
        viewModel.Context = context;
    }

    private async Task<Pager> GetPagerAsync(BuildPartDisplayContext context)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var pagerParameters = new PagerParameters();
        await context.Updater.TryUpdateModelAsync(pagerParameters);

        var pager = new Pager(pagerParameters, siteSettings.PageSize);

        return pager;
    }
}
