using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class TaxRateTaxPartDisplayDriver : ContentPartDisplayDriver<TaxPart>
{
    private readonly TaxRateTaxProvider _taxRateTaxProvider;

    public TaxRateTaxPartDisplayDriver(TaxRateTaxProvider taxRateTaxProvider) =>
        _taxRateTaxProvider = taxRateTaxProvider;

    public override async Task<IDisplayResult> DisplayAsync(TaxPart part, BuildPartDisplayContext context)
    {
        if (part.As<ProductPart>() is not { } product ||
            part.As<PricePart>()?.Price is not { } netUnitPrice)
        {
            return null;
        }

        var model = PromotionAndTaxProviderContext.SingleProduct(product, netUnitPrice);
        if (!await _taxRateTaxProvider.IsApplicableAsync(model)) return null;

        model = await _taxRateTaxProvider.UpdateAsync(model);

        return Initialize<TaxRateViewModel>("TaxPart_TaxRate_GrossPrice", viewModel =>
            {
                viewModel.Context = model;
            })
            .Location("Detail", "Content");
    }
}
