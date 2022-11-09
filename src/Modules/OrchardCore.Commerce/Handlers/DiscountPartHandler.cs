using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers;

public class DiscountPartHandler : ContentPartHandler<DiscountPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizer<TaxPartAndPricePartHandler> T;

    public DiscountPartHandler(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<TaxPartAndPricePartHandler> stringLocalizer,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _updateModelAccessor = updateModelAccessor;
        T = stringLocalizer;
    }

    public override Task UpdatedAsync(UpdateContentContext context, DiscountPart instance)
    {
        if (instance.ContentItem.As<DiscountPart>() is not { } discountPart) return Task.CompletedTask;

        var discountPercentage = discountPart.Percentage?.Value ?? 0;
        var discountAmount = discountPart.Amount;

        // IsValid allows 0 value, if the percentage is 0 it's not present.
        var isDiscountAmountPresent = discountAmount.IsValid && discountAmount.Value > 0;

        var isDiscountPercentagePresent = discountPercentage > 0;

        if (isDiscountPercentagePresent && isDiscountAmountPresent)
        {
            InvalidateUnevenState();
        }

        return Task.CompletedTask;
    }

    private void InvalidateUnevenState()
    {
        var definition = _contentDefinitionManager.GetPartDefinition(nameof(DiscountPart));
        var percentageName = definition.Fields
            .Single(field => field.Name == nameof(DiscountPart.Percentage)).DisplayName();
        var amountName = definition.Fields.Single(field => field.Name == nameof(DiscountPart.Amount)).DisplayName();

        _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
            nameof(TaxPart.GrossPrice),
            T["You must either provide only {0}, or {1}, or neither of them.", percentageName, amountName]);
    }
}
