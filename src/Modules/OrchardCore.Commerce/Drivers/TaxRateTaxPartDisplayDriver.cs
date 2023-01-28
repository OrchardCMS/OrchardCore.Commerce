using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class TaxRateTaxPartDisplayDriver : ContentPartDisplayDriver<TaxPart>
{
    private readonly IHttpContextAccessor _hca;
    private readonly IEnumerable<ITaxProvider> _taxProviders;

    public TaxRateTaxPartDisplayDriver(
        IHttpContextAccessor hca,
        IEnumerable<ITaxProvider> taxProviders)
    {
        _hca = hca;
        _taxProviders = taxProviders;
    }

    public override async Task<IDisplayResult> DisplayAsync(TaxPart part, BuildPartDisplayContext context)
    {
        if (part.As<ProductPart>() is not { } product ||
            part.As<PricePart>()?.Price is not { } netUnitPrice ||
            _hca.HttpContext is not { } httpContext)
        {
            return null;
        }

        var userAddresses = await httpContext.GetUserAddressAsync();
        var model = PromotionAndTaxProviderContext.SingleProduct(
            product,
            netUnitPrice,
            userAddresses?.ShippingAddress.Address,
            userAddresses?.BillingAddress.Address);

        if (await _taxProviders.GetFirstApplicableProviderAsync(model) is not TaxRateTaxProvider taxRateTaxProvider)
        {
            return null;
        }

        httpContext.Items[nameof(TaxRateTaxPartDisplayDriver)] = true;

        model = await taxRateTaxProvider.UpdateAsync(model);
        return Initialize<TaxRateViewModel>("TaxPart_TaxRate_GrossPrice", viewModel =>
            {
                viewModel.Context = model;
            })
            .Location("Detail", "Content");
    }
}
