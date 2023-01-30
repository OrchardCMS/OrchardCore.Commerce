using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Promotion.ViewModels;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Drivers;

public class DiscountPartDisplayDriver : ContentPartDisplayDriver<DiscountPart>
{
    public const string DiscountPartContextItemKey = $"{nameof(DiscountPartDisplayDriver)}:{nameof(DiscountPart)}";

    private readonly IHttpContextAccessor _hca;

    public DiscountPartDisplayDriver(IHttpContextAccessor hca) => _hca = hca;

    public override IDisplayResult Display(DiscountPart part, BuildPartDisplayContext context) =>
        _hca.HttpContext?.Items.GetMaybe(DiscountPartContextItemKey) is not IEnumerable<DiscountPart> &&
        part.IsValidAndActive() &&
        CalculateNewPrice(part) is { IsValid: true } newPrice
            ? Initialize<DiscountPartViewModel>(nameof(DiscountPart), viewModel => BuildViewModel(viewModel, part, newPrice))
                .Location("Detail", "Content:20")
                .Location("Summary", "Meta:5")
            : null;

    public override IDisplayResult Edit(DiscountPart part, BuildPartEditorContext context) =>
        Initialize<DiscountPartViewModel>(GetEditorShapeType(context), viewModel => BuildViewModel(viewModel, part, newPrice: null));

    private static void BuildViewModel(DiscountPartViewModel model, DiscountPart part, Amount? newPrice)
    {
        model.ContentItem = part.ContentItem;
        model.DiscountPart = part;

        if (newPrice != null) model.NewPrice.Amount = newPrice.Value;

        model.OldPriceClassNames.Add("tax-part-gross-price-value");
        model.OldPriceClassNames.Add("price-part-price-field-value");
    }

    private static Amount? CalculateNewPrice(DiscountPart part)
    {
        var contentItem = part.ContentItem;
        var newPrice = contentItem?.As<TaxPart>()?.GrossPrice?.Amount is { } grossPrice && grossPrice.IsValid
            ? grossPrice
            : contentItem?.As<PricePart>()?.Price;

        var discountPercentage = part.DiscountPercentage.Value;
        var discountAmount = part.DiscountAmount.Amount;

        if (newPrice is not { } notNullPrice) return null;

        if (discountPercentage is { } and not 0)
        {
            return notNullPrice.WithDiscount((decimal)discountPercentage);
        }

        if (discountAmount.IsValidAndNonZero)
        {
            return notNullPrice.WithDiscount(discountAmount);
        }

        return null;
    }

    public class StoredDiscountPartDisplayDriver : ContentPartDisplayDriver<ProductPart>
    {
        private readonly IHttpContextAccessor _hca;
        public StoredDiscountPartDisplayDriver(IHttpContextAccessor hca) => _hca = hca;

        public override IDisplayResult Display(ProductPart part, BuildPartDisplayContext context) =>
            _hca.HttpContext?.Items.GetMaybe(DiscountPartContextItemKey) is IEnumerable<DiscountPart> storedParts
                ? new CombinedResult(storedParts.Select(storedPart =>
                    new DiscountPartDisplayDriver(_hca).Display(storedPart, context)))
                : null;
    }
}
