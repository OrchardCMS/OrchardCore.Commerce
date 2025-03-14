using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class PriceVariantsPartDisplayDriver : ContentPartDisplayDriver<PriceVariantsPart>
{
    private readonly IMoneyService _moneyService;
    private readonly IPredefinedValuesProductAttributeService _predefinedValuesProductAttributeService;
    private readonly IOptions<CurrencySettings> _currencyOptions;

    public PriceVariantsPartDisplayDriver(
        IMoneyService moneyService,
        IPredefinedValuesProductAttributeService predefinedValuesProductAttributeService,
        IOptions<CurrencySettings> currencyOptions)
    {
        _moneyService = moneyService;
        _predefinedValuesProductAttributeService = predefinedValuesProductAttributeService;
        _currencyOptions = currencyOptions;
    }

    public override IDisplayResult Display(PriceVariantsPart part, BuildPartDisplayContext context) =>
        Initialize<PriceVariantsPartViewModel>(GetDisplayShapeType(context), async viewModel => await BuildViewModelAsync(viewModel, part))
            .Location("Detail", "Content:25")
            .Location("Summary", "Meta:10");

    public override IDisplayResult Edit(PriceVariantsPart part, BuildPartEditorContext context) =>
        Initialize<PriceVariantsPartViewModel>(GetEditorShapeType(context), async viewModel =>
        {
            await BuildViewModelAsync(viewModel, part);
            viewModel.Currencies = _moneyService.Currencies;
        });

    public override async Task<IDisplayResult> UpdateAsync(
        PriceVariantsPart part,
        UpdatePartEditorContext context)
    {
        var viewModel = new PriceVariantsPartViewModel();
        await context.Updater.TryUpdateModelAsync(
            viewModel,
            Prefix,
            viewModel => viewModel.VariantsValues,
            viewModel => viewModel.VariantsCurrencies);

        // Restoring variants so that only the new values are stored.
        part.Variants.RemoveAll();
        viewModel.Variants.RemoveAll();

        foreach ((string key, decimal? value) in viewModel.VariantsValues)
        {
            if (value.HasValue &&
                viewModel.VariantsCurrencies?.ContainsKey(key) == true &&
                viewModel.VariantsCurrencies[key] != Currency.UnspecifiedCurrency.CurrencyIsoCode)
            {
                part.Variants[key] = _moneyService.Create(value.Value, viewModel.VariantsCurrencies[key]);
            }
        }

        return await EditAsync(part, context);
    }

    private async Task BuildViewModelAsync(PriceVariantsPartViewModel model, PriceVariantsPart part)
    {
        model.ContentItem = part.ContentItem;
        model.PriceVariantsPart = part;

        var allVariantsKeys = (await _predefinedValuesProductAttributeService.GetProductAttributesCombinationsAsync(part.ContentItem))
            .Select(attr => attr.HtmlClassify().ToUpperInvariant())
            .ToList();

        var variants = part.Variants ?? new Dictionary<string, Amount>();

        var values = allVariantsKeys.ToDictionary(
            key => key,
            key => part.Variants.TryGetValue(key, out var amount)
                ? amount.Value
                : (decimal?)null);

        var currencies = allVariantsKeys.ToDictionary(
            key => key,
            key => part.Variants.TryGetValue(key, out var amount)
                ? amount.Currency.CurrencyIsoCode
                : _currencyOptions.Value.CurrentDisplayCurrency);

        model.InitializeVariants(variants, values, currencies);

        // When creating a new PriceVariantsProduct item, initialize default inventories.
        if (part.ContentItem.As<InventoryPart>() is { } inventoryPart && !inventoryPart.Inventory.Any())
        {
            foreach (var variantKey in allVariantsKeys)
            {
                inventoryPart.Inventory.Add(variantKey, 0);
                inventoryPart.InventoryKeys.Add(variantKey);
            }
        }
    }
}
