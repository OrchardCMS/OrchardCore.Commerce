using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers
{
    public class PriceVariantsPartDisplayDriver : ContentPartDisplayDriver<PriceVariantsPart>
    {
        private readonly IMoneyService _moneyService;
        private readonly IPredefinedValuesProductAttributeService _predefinedValuesProductAttributeService;

        public PriceVariantsPartDisplayDriver(IMoneyService moneyService, IPredefinedValuesProductAttributeService predefinedValuesProductAttributeService)
        {
            _moneyService = moneyService;
            _predefinedValuesProductAttributeService = predefinedValuesProductAttributeService;
        }

        public override IDisplayResult Display(PriceVariantsPart part, BuildPartDisplayContext context)
        {
            return Initialize<PriceVariantsPartViewModel>(GetDisplayShapeType(context), m => BuildViewModel(m, part))
                .Location("Detail", "Content:25")
                .Location("Summary", "Meta:10");
        }

        public override IDisplayResult Edit(PriceVariantsPart part, BuildPartEditorContext context)
        {
            return Initialize<PriceVariantsPartViewModel>(GetEditorShapeType(context), m =>
            {
                BuildViewModel(m, part);
                m.Currencies = _moneyService.Currencies;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(PriceVariantsPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var updateModel = new PriceVariantsPartViewModel();
            if (await updater.TryUpdateModelAsync(updateModel, Prefix, t => t.VariantsValues, t => t.VariantsCurrencies))
            {
                // Remove any content or the variants would be merged and not be cleared
                part.Content.Variants.RemoveAll();

                part.Variants = updateModel.VariantsValues
                    .Where(x => x.Value.HasValue
                        && updateModel.VariantsCurrencies?.ContainsKey(x.Key) == true
                        && updateModel.VariantsCurrencies[x.Key] != Currency.UnspecifiedCurrency.CurrencyIsoCode)
                    .ToDictionary(x => x.Key,
                        x => _moneyService.Create(x.Value.Value, updateModel.VariantsCurrencies[x.Key]));
            }

            return Edit(part, context);
        }

        private Task BuildViewModel(PriceVariantsPartViewModel model, PriceVariantsPart part)
        {
            model.ContentItem = part.ContentItem;
            model.PriceVariantsPart = part;

            var allVariantsKeys = _predefinedValuesProductAttributeService.GetProductAttributesCombinations(part.ContentItem);
            model.Variants = part.Variants ?? new Dictionary<string, Amount>();

            model.VariantsValues = allVariantsKeys.ToDictionary(x => x,
                x => model.Variants.TryGetValue(x, out var amount)
                    ? new decimal?(amount.Value)
                    : null);

            model.VariantsCurrencies = allVariantsKeys.ToDictionary(x => x,
                x => model.Variants.TryGetValue(x, out var amount)
                    ? amount.Currency.CurrencyIsoCode
                    : Currency.UnspecifiedCurrency.CurrencyIsoCode);

            return Task.CompletedTask;
        }
    }
}
