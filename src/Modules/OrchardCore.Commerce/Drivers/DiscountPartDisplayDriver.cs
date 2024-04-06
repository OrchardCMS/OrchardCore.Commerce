using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Lombiq.HelpfulLibraries.OrchardCore.Validation;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.Promotion.Extensions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Promotion.ViewModels;
using OrchardCore.Commerce.Tax.Extensions;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using System.Linq;
using System.Threading.Tasks;
using FrontendException=Lombiq.HelpfulLibraries.AspNetCore.Exceptions.FrontendException;

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
            if (context.DisplayType == CommonContentDisplayTypes.SummaryAdmin) return null;

            try
            {
                var model = await _shoppingCartHelpers.EstimateProductAsync(shoppingCartId: null, part.Sku);
                var data = model.AdditionalData;

                var discounts = data.GetDiscounts().ToList();
                if (discounts.Count == 0) return null;

                var shapes = discounts
                    .Select(discount => Initialize<DiscountPartViewModel>(
                            nameof(DiscountPart),
                            viewModel => BuildViewModel(viewModel, discount, part, newPrice: null))
                        .Location("Detail", "Content:20"))
                    .ToList();

                shapes.Add(Initialize<DiscountPartUpdateScriptViewModel>(
                        nameof(DiscountPart) + "_UpdateScript",
                        viewModel =>
                        {
                            var (oldNetPrice, oldGrossPrice) = data.GetOldPrices();
                            if (data.HasGrossPrice() && oldGrossPrice is { } oldGrossPriceValue)
                            {
                                viewModel.Add(".price-part-price-field-value", oldNetPrice, data.GetNetPrice());
                                viewModel.Add(".tax-rate-gross-price-value", oldGrossPriceValue, data.GetGrossPrice());
                            }
                            else
                            {
                                viewModel.Add(".price-part-price-field-value", oldNetPrice, model.UnitPrice);
                            }
                        })
                    .Location("Detail", "Content:20"));

                return new CombinedResult(shapes);
            }
            catch (FrontendException exception)
            {
                await _notifier.FrontEndErrorAsync(exception);
                return null;
            }
        }
    }
}
