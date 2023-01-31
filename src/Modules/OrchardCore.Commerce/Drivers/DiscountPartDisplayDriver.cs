using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Exceptions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Promotion.ViewModels;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class DiscountPartDisplayDriver : ContentPartDisplayDriver<DiscountPart>
{
    public override IDisplayResult Display(DiscountPart part, BuildPartDisplayContext context) =>
        part.IsValidAndActive() && CalculateNewPrice((DiscountInformation)part, part) is { IsValid: true } newPrice
            ? Initialize<DiscountPartViewModel>(nameof(DiscountPart), viewModel =>
                    BuildViewModel(viewModel, (DiscountInformation)part, part, newPrice))
                .Location("Summary", "Meta:5")
            : null;

    public override IDisplayResult Edit(DiscountPart part, BuildPartEditorContext context) =>
        Initialize<DiscountPartViewModel>(GetEditorShapeType(context), viewModel =>
            BuildViewModel(viewModel, (DiscountInformation)part, part, newPrice: null));

    private static void BuildViewModel(DiscountPartViewModel model, DiscountInformation discount, IContent content, Amount? newPrice)
    {
        model.ContentItem = content.ContentItem;
        model.Discount = discount;

        if (newPrice != null) model.NewPrice.Amount = newPrice.Value;

        model.OldPriceClassNames.Add("tax-part-gross-price-value");
        model.OldPriceClassNames.Add("price-part-price-field-value");
    }

    private static Amount? CalculateNewPrice(DiscountInformation discount, IContent content)
    {
        var contentItem = content?.ContentItem;
        var newPrice = contentItem?.As<TaxPart>()?.GrossPrice?.Amount is { IsValid: true } grossPrice
            ? grossPrice
            : contentItem?.As<PricePart>()?.Price;

        if (newPrice is not { } notNullPrice) return null;
        if (discount.DiscountPercentage > 0) return notNullPrice.WithDiscount(discount.DiscountPercentage);
        if (discount.DiscountAmount.IsValidAndNonZero) return notNullPrice.WithDiscount(discount.DiscountAmount);

        return null;
    }

    public class StoredDiscountPartDisplayDriver : ContentPartDisplayDriver<ProductPart>
    {
        private readonly INotifier _notifier;
        private readonly IShoppingCartHelpers _shoppingCartHelpers;

        public StoredDiscountPartDisplayDriver(INotifier notifier, IShoppingCartHelpers shoppingCartHelpers)
        {
            _notifier = notifier;
            _shoppingCartHelpers = shoppingCartHelpers;
        }

        public override async Task<IDisplayResult> DisplayAsync(ProductPart part, BuildPartDisplayContext context)
        {
            try
            {
                var model = await _shoppingCartHelpers.EstimateProductAsync(
                    shoppingCartId: null,
                    new ShoppingCartItem(
                        quantity: 1,
                        part.Sku));

                var discounts = model
                    .AdditionalData
                    .GetDiscounts()
                    .SelectWhere(
                        discount => new
                        {
                            Discount = discount,
                            NewPrice = discount.IsValidAndActive() && CalculateNewPrice(discount, part) is { IsValid: true } newPrice
                                ? newPrice
                                : Amount.Unspecified,
                        },
                        item => item.NewPrice.IsValid)
                    .Select(item => Initialize<DiscountPartViewModel>(
                            nameof(DiscountPart),
                            viewModel => BuildViewModel(viewModel, item.Discount, part, item.NewPrice))
                        .Location("Detail", "Content:20"));

                return new CombinedResult(discounts);
            }
            catch (FrontendException exception)
            {
                await _notifier.ErrorAsync(exception.HtmlMessage);
                return null;
            }
        }
    }
}
