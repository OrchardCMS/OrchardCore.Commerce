using Money;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class PriceVariantsPartDisplayDriver : ContentPartDisplayDriver<PriceVariantsPart>
{
    private readonly IMoneyService _moneyService;
    private readonly IPredefinedValuesProductAttributeService _predefinedValuesProductAttributeService;

    public PriceVariantsPartDisplayDriver(
        IMoneyService moneyService,
        IPredefinedValuesProductAttributeService predefinedValuesProductAttributeService)
    {
        _moneyService = moneyService;
        _predefinedValuesProductAttributeService = predefinedValuesProductAttributeService;
    }

    public override IDisplayResult Display(PriceVariantsPart part, BuildPartDisplayContext context) =>
        Initialize<PriceVariantsPartViewModel>(GetDisplayShapeType(context), viewModel => BuildViewModel(viewModel, part))
            .Location("Detail", "Content:25")
            .Location("Summary", "Meta:10");

    public override IDisplayResult Edit(PriceVariantsPart part, BuildPartEditorContext context) =>
        Initialize<PriceVariantsPartViewModel>(GetEditorShapeType(context), viewModel =>
        {
            BuildViewModel(viewModel, part);
            viewModel.Currencies = _moneyService.Currencies;
        });

    public override async Task<IDisplayResult> UpdateAsync(
        PriceVariantsPart part,
        IUpdateModel updater,
        UpdatePartEditorContext context)
    {
        var updateModel = new PriceVariantsPartViewModel();
        if (await updater.TryUpdateModelAsync(
                updateModel,
                Prefix,
                viewModel => viewModel.VariantsValues,
                viewModel => viewModel.VariantsCurrencies))
        {
            // Remove any content or the variants would be merged and not be cleared
            part.Content.Variants.RemoveAll();

            foreach (var x in updateModel.VariantsValues)
            {
                if (x.Value.HasValue &&
                    updateModel.VariantsCurrencies?.ContainsKey(x.Key) == true &&
                    updateModel.VariantsCurrencies[x.Key] != Currency.UnspecifiedCurrency.CurrencyIsoCode)
                {
                    part.Variants[x.Key] = _moneyService.Create(x.Value.Value, updateModel.VariantsCurrencies[x.Key]);
                }
            }
        }

        return await EditAsync(part, context);
    }

    private void BuildViewModel(PriceVariantsPartViewModel model, PriceVariantsPart part)
    {
        model.ContentItem = part.ContentItem;
        model.PriceVariantsPart = part;

        var allVariantsKeys = _predefinedValuesProductAttributeService
            .GetProductAttributesCombinations(part.ContentItem)
            .ToList();

        var variants = part.Variants ?? new Dictionary<string, Amount>();

        var values = allVariantsKeys.ToDictionary(
            key => key,
            key => model.Variants.TryGetValue(key, out var amount)
                ? amount.Value
                : (decimal?)null);

        var currencies = allVariantsKeys.ToDictionary(
            key => key,
            key => model.Variants.TryGetValue(key, out var amount)
                ? amount.Currency.CurrencyIsoCode
                : Currency.UnspecifiedCurrency.CurrencyIsoCode);

        model.InitializeVariants(variants, values, currencies);
    }
}
