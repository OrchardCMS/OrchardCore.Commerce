using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Handlers;

public class DiscountPartHandler : CreatingOrUpdatingPartHandler<DiscountPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizer<DiscountPart> T;

    public DiscountPartHandler(
        IContentDefinitionManager contentDefinitionManager,
        IStringLocalizer<DiscountPart> stringLocalizer,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _updateModelAccessor = updateModelAccessor;
        T = stringLocalizer;
    }

    protected override async Task CreatingOrUpdatingAsync(DiscountPart part)
    {
        if (part.ContentItem.As<DiscountPart>() is not { } discountPart) return;

        var discountPercentage = discountPart.DiscountPercentage?.Value ?? 0;
        var discountAmount = discountPart.DiscountAmount.Amount;

        var isDiscountPercentagePresent = discountPercentage > 0;

        if (isDiscountPercentagePresent && discountAmount.IsValidAndNonZero)
        {
            await InvalidateEvenStateAsync();
        }

        if ((part.ContentItem.As<PricePart>()?.Price is { } pricePartPrice &&
            pricePartPrice.Currency.Equals(discountAmount.Currency) &&
            pricePartPrice < discountAmount) ||
            (part.ContentItem.As<TaxPart>()?.GrossPrice.Amount is { IsValid: true } taxPartGrossPriceAmount &&
            taxPartGrossPriceAmount.Currency.Equals(discountAmount.Currency) &&
            taxPartGrossPriceAmount < discountAmount))
        {
            await InvalidateNegativePriceStateAsync();
        }
    }

    private async Task InvalidateEvenStateAsync()
    {
        var definition = await _contentDefinitionManager.GetPartDefinitionAsync(nameof(DiscountPart));
        var percentageName = definition.Fields
            .Single(field => field.Name == nameof(DiscountPart.DiscountPercentage)).DisplayName();

        var amountName = definition.Fields
            .Single(field => field.Name == nameof(DiscountPart.DiscountAmount)).DisplayName();

        _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
            nameof(DiscountPart.DiscountPercentage),
            T["You must either provide only {0}, or {1}, or neither of them.", percentageName, amountName]);
    }

    private async Task InvalidateNegativePriceStateAsync()
    {
        var definition = await _contentDefinitionManager.GetPartDefinitionAsync(nameof(DiscountPart));

        var amountName = definition.Fields
            .Single(field => field.Name == nameof(DiscountPart.DiscountAmount)).DisplayName();

        _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
            nameof(DiscountPart.DiscountAmount),
            T["Discount amount must be smaller than the product's original price.", amountName]);
    }
}
