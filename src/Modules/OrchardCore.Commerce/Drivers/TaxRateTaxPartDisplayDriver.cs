using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class TaxRateTaxPartDisplayDriver : ContentPartDisplayDriver<TaxPart>
{
    private readonly ISiteService _siteService;
    private readonly TaxRateTaxProvider _taxRateTaxProvider;
    private readonly IStringLocalizer T;

    public TaxRateTaxPartDisplayDriver(
        ISiteService siteService,
        TaxRateTaxProvider taxRateTaxProvider,
        IStringLocalizer<TaxRateTaxPartDisplayDriver> stringLocalizer)
    {
        _siteService = siteService;
        _taxRateTaxProvider = taxRateTaxProvider;
        T = stringLocalizer;
    }

    public override async Task<IDisplayResult> DisplayAsync(TaxPart part, BuildPartDisplayContext context)
    {
        var combined = new CombinedResult();
        var siteSettings = await _siteService.GetSiteSettingsAsync();

        if (siteSettings.As<TaxRateSetting>() is { } taxRateSetting)
        {
            Initialize<TaxRateViewModel>(GetDisplayShapeType(context), viewModel =>
                {

                })
                .Location("Detail", "Content");
        }

        return combined;
    }
}
