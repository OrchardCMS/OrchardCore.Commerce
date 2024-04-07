using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Lombiq.HelpfulLibraries.OrchardCore.Validation;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Tax.Extensions;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

using FrontendException = Lombiq.HelpfulLibraries.AspNetCore.Exceptions.FrontendException;

namespace OrchardCore.Commerce.Drivers;

public class TaxRateTaxPartDisplayDriver : ContentPartDisplayDriver<TaxPart>
{
    private readonly IHttpContextAccessor _hca;
    private readonly INotifier _notifier;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;

    public TaxRateTaxPartDisplayDriver(
        IHttpContextAccessor hca,
        INotifier notifier,
        IShoppingCartHelpers shoppingCartHelpers)
    {
        _hca = hca;
        _notifier = notifier;
        _shoppingCartHelpers = shoppingCartHelpers;
    }

    public override async Task<IDisplayResult> DisplayAsync(TaxPart part, BuildPartDisplayContext context)
    {
        if (part.As<ProductPart>() is not { } product || _hca.HttpContext is not { } httpContext) return null;

        try
        {
            var addresses = await _hca.HttpContext.GetUserAddressAsync();
            var model = await _shoppingCartHelpers.EstimateProductAsync(
                shoppingCartId: null,
                product.Sku,
                addresses?.GetSafeShippingAddress(),
                addresses?.GetSafeBillingAddress());

            if (!model.AdditionalData.HasGrossPrice()) return null;

            httpContext.Items[nameof(TaxRateTaxPartDisplayDriver)] = true;

            return Initialize<TaxRateViewModel>("TaxPart_TaxRate_GrossPrice", viewModel =>
                viewModel.Context = new PromotionAndTaxProviderContext(
                    new[] { new PromotionAndTaxProviderContextLineItem(model) },
                    new[] { model.LinePrice },
                    addresses?.GetSafeShippingAddress(),
                    addresses?.GetSafeBillingAddress()))
                .Location(CommonContentDisplayTypes.Detail, CommonLocationNames.Content);
        }
        catch (FrontendException exception)
        {
            await _notifier.FrontEndErrorAsync(exception);
            return null;
        }
    }
}
