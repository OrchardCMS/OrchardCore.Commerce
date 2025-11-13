using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Handlers;

public class TaxPartAndPricePartHandler : ContentPartHandler<PricePart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizer<TaxPartAndPricePartHandler> T;

    public TaxPartAndPricePartHandler(
        IContentDefinitionManager contentDefinitionManager,
        ISession session,
        IStringLocalizer<TaxPartAndPricePartHandler> stringLocalizer,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _session = session;
        _updateModelAccessor = updateModelAccessor;
        T = stringLocalizer;
    }

    public override async Task UpdatedAsync(UpdateContentContext context, PricePart part)
    {
        if (part.ContentItem.As<TaxPart>() is not { } taxPart) return;

        var taxRate = taxPart.TaxRate?.Value ?? 0;

        var isGrossPricePresent = taxPart.GrossPrice?.Amount.IsValid == true;
        var isTaxRatePresent = taxRate >= 0;

        if (isGrossPricePresent && isTaxRatePresent)
        {
            await UpdatePricePartAsync(part.ContentItem, taxPart.GrossPrice.Amount, taxRate);
        }
        else if (isGrossPricePresent ^ isTaxRatePresent)
        {
            await InvalidateUnevenStateAsync();
        }

        return;
    }

    private Task UpdatePricePartAsync(ContentItem contentItem, Amount grossPrice, decimal taxRate)
    {
        contentItem.Alter<PricePart>(instance => instance.PriceField.Amount = grossPrice.WithoutTax(taxRate));
        return _session.SaveAsync(contentItem);
    }

    private async Task InvalidateUnevenStateAsync()
    {
        var definition = await _contentDefinitionManager.GetPartDefinitionAsync(nameof(TaxPart));
        var grossPriceName = definition.Fields.Single(field => field.Name == nameof(TaxPart.GrossPrice)).DisplayName();
        var taxRateName = definition.Fields.Single(field => field.Name == nameof(TaxPart.TaxRate)).DisplayName();

        _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
            nameof(TaxPart.GrossPrice),
            T["You must either provide both {0} and {1}, or neither of them.", grossPriceName, taxRateName]);
    }
}
