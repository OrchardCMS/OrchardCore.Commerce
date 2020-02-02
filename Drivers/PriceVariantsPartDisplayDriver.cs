using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class PriceVariantsPartDisplayDriver : ContentPartDisplayDriver<PriceVariantsPart>
    {
        private readonly IMoneyService _moneyService;
        private IPriceVariantsService _priceVariantsService;

        public PriceVariantsPartDisplayDriver(IMoneyService moneyService, IPriceVariantsService priceVariantsService)
        {
            _moneyService = moneyService;
            _priceVariantsService = priceVariantsService;
        }

        public override IDisplayResult Display(PriceVariantsPart pricePart)
        {
            return Combine(
                Initialize<PriceVariantsPartViewModel>("PriceVariantsPart", m => BuildViewModel(m, pricePart))
                    .Location("Detail", "Content:25"),
                Initialize<PriceVariantsPartViewModel>("PriceVariantsPart_Summary", m => BuildViewModel(m, pricePart))
                    .Location("Summary", "Meta:10")
            );
        }

        public override IDisplayResult Edit(PriceVariantsPart pricePart)
        {
            return Initialize<PriceVariantsPartViewModel>("PriceVariantsPart_Edit", m => BuildViewModel(m, pricePart));
        }

        public override async Task<IDisplayResult> UpdateAsync(PriceVariantsPart model, IUpdateModel updater)
        {
            var updateModel = new PriceVariantsPartViewModel();
            await updater.TryUpdateModelAsync(updateModel, Prefix, t => t.VariantsValues);
            await updater.TryUpdateModelAsync(updateModel, Prefix, t => t.VariantsCurrencies);
            model.Variants = updateModel.VariantsValues.ToDictionary(x => x.Key, x => _moneyService.Create(x.Value, updateModel.VariantsCurrencies[x.Key]));

            return Edit(model);
        }

        private Task BuildViewModel(PriceVariantsPartViewModel model, PriceVariantsPart part)
        {
            model.ContentItem = part.ContentItem;

            model.PriceVariantsPart = part;
            model.Currencies = _moneyService.Currencies;

            model.Variants = _priceVariantsService.GetPriceVariants(part.ContentItem);
            model.VariantsValues = model.Variants.ToDictionary(x => x.Key, x => x.Value.Value);
            model.VariantsCurrencies = model.Variants.ToDictionary(x => x.Key, x => x.Value.Currency.CurrencyIsoCode);

            return Task.CompletedTask;
        }
    }
}
