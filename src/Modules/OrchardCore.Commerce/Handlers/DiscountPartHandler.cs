using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
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

    public override Task UpdatedAsync(UpdateContentContext context, DiscountPart instance)
    {
        if (instance.ContentItem.As<DiscountPart>() is not { } discountPart) return Task.CompletedTask;

        var discountPercentage = discountPart.DiscountPercentage?.Value ?? 0;
        var discountAmount = discountPart.DiscountAmount.Amount;

        // IsValid allows 0 value, but if the percentage is 0 it's not present.
        var isDiscountAmountPresent = discountAmount.IsValidAndPositive();

        var isDiscountPercentagePresent = discountPercentage > 0;

        if (isDiscountPercentagePresent && isDiscountAmountPresent)
        {
            InvalidateEvenState();
        }

        if ((instance.ContentItem.As<PricePart>()?.Price is { } pricePartPrice &&
            pricePartPrice.Currency.Equals(discountAmount.Currency) &&
            pricePartPrice < discountAmount) ||
            (instance.ContentItem.As<TaxPart>()?.GrossPrice.Amount is { } taxPartGrossPriceAmount &&
            taxPartGrossPriceAmount.IsValid &&
            taxPartGrossPriceAmount.Currency.Equals(discountAmount.Currency) &&
            taxPartGrossPriceAmount < discountAmount))
        {
            InvalidateNegativePriceState();
        }

        return Task.CompletedTask;
    }

    private void InvalidateEvenState()
    {
        var definition = _contentDefinitionManager.GetPartDefinition(nameof(DiscountPart));
        var percentageName = definition.Fields
            .Single(field => field.Name == nameof(DiscountPart.DiscountPercentage)).DisplayName();

        var amountName = definition.Fields
            .Single(field => field.Name == nameof(DiscountPart.DiscountAmount)).DisplayName();

        _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
            nameof(DiscountPart.DiscountPercentage),
            T["You must either provide only {0}, or {1}, or neither of them.", percentageName, amountName]);
    }

    private void InvalidateNegativePriceState()
    {
        var definition = _contentDefinitionManager.GetPartDefinition(nameof(DiscountPart));

        var amountName = definition.Fields
            .Single(field => field.Name == nameof(DiscountPart.DiscountAmount)).DisplayName();

        _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
            nameof(DiscountPart.DiscountAmount),
            T["Discount amount must be smaller than the product's original price.", amountName]);
    }
}
