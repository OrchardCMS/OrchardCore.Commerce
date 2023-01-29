using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Promotion.ViewModels;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers;

public class DiscountPartDisplayDriver : ContentPartDisplayDriver<DiscountPart>
{
    public override IDisplayResult Display(DiscountPart part, BuildPartDisplayContext context) =>
        Initialize<DiscountPartViewModel>(GetDisplayShapeType(context), viewModel => BuildViewModel(viewModel, part))
            .Location("Detail", "Content:20")
            .Location("Summary", "Meta:5");

    public override IDisplayResult Edit(DiscountPart part, BuildPartEditorContext context) =>
        Initialize<DiscountPartViewModel>(GetEditorShapeType(context), viewModel => BuildViewModel(viewModel, part));

    private static void BuildViewModel(DiscountPartViewModel model, DiscountPart part)
    {
        var contentItem = part.ContentItem;
        model.ContentItem = contentItem;

        model.DiscountPart = part;

        var discountPercentage = part.DiscountPercentage.Value;
        var discountAmount = part.DiscountAmount.Amount;

        var newPrice = contentItem?.As<TaxPart>()?.GrossPrice?.Amount is { } grossPrice && grossPrice.IsValid
            ? grossPrice
            : contentItem?.As<PricePart>()?.Price;

        if (newPrice is { } notNullPrice)
        {
            if (discountPercentage is { } and not 0)
            {
                notNullPrice = notNullPrice.WithDiscount((decimal)discountPercentage);
            }

            if (discountAmount.IsValidAndNonZero)
            {
                notNullPrice = notNullPrice.WithDiscount(discountAmount);
            }

            model.NewPrice.Amount = notNullPrice;
        }

        model.OldPriceClassNames.Add("tax-part-gross-price-value");
        model.OldPriceClassNames.Add("price-part-price-field-value");
    }
}
