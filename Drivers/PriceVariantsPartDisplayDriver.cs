using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
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
            await updater.TryUpdateModelAsync(updateModel, Prefix, t => t.BasePriceValue);
            await updater.TryUpdateModelAsync(updateModel, Prefix, t => t.PriceCurrency);
            await updater.TryUpdateModelAsync(updateModel, Prefix, t => t.VariantsValues);
            model.BasePrice = _moneyService.Create(updateModel.BasePriceValue, updateModel.PriceCurrency);
            model.Variants = updateModel.VariantsValues;

            return Edit(model);
        }

        private Task BuildViewModel(PriceVariantsPartViewModel model, PriceVariantsPart part)
        {
            model.ContentItem = part.ContentItem;

            var priceCurrency = part.BasePrice.Currency ?? _moneyService.DefaultCurrency;
            model.BasePrice = part.BasePrice;
            model.BasePriceValue = part.BasePrice.Value;
            model.PriceCurrency = priceCurrency.CurrencyIsoCode;
            model.PriceVariantsPart = part;
            model.Currencies = _moneyService.Currencies;

            model.VariantsValues = _priceVariantsService.GetPriceVariants(part.ContentItem);
            model.Variants = model.VariantsValues.ToDictionary(x => x.Key, x => new Amount(x.Value, priceCurrency));

            return Task.CompletedTask;
        }
    }
}
