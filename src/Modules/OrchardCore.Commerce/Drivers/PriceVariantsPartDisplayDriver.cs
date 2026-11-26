using Lombiq.HelpfulLibraries.OrchardCore.Contents;
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
    private readonly IProductAttributeService _productAttributeService;
    private readonly IOptions<CurrencySettings> _currencyOptions;

    public PriceVariantsPartDisplayDriver(
        IMoneyService moneyService,
        IProductAttributeService productAttributeService,
        IOptions<CurrencySettings> currencyOptions)
    {
        _moneyService = moneyService;
        _productAttributeService = productAttributeService;
        _currencyOptions = currencyOptions;
    }

    public override IDisplayResult Display(PriceVariantsPart part, BuildPartDisplayContext context) =>
        Initialize<PriceVariantsPartViewModel>(
                GetDisplayShapeType(context),
                async viewModel => await BuildViewModelAsync(viewModel, part, cleanForDisplay: true))
            .Location(CommonContentDisplayTypes.Detail, "Content:25")
            .Location(CommonContentDisplayTypes.Summary, "Meta:10");

    public override IDisplayResult Edit(PriceVariantsPart part, BuildPartEditorContext context) =>
        Initialize<PriceVariantsPartViewModel>(GetEditorShapeType(context), async viewModel =>
        {
            await BuildViewModelAsync(viewModel, part);
            viewModel.Currencies = _moneyService.Currencies;

            if (part.ContentItem.Has<InventoryPart>())
            {
                await _productAttributeService.UpdateInventoryPartFromProductAttributesAsync(part.ContentItem);
            }
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

        foreach (var (key, value) in viewModel.VariantsValues)
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

    private async Task BuildViewModelAsync(
        PriceVariantsPartViewModel model,
        PriceVariantsPart part,
        bool cleanForDisplay = false)
    {
        var product = part.ContentItem;
        model.ContentItem = product;
        model.PriceVariantsPart = part;

        var allVariantsKeys = (await _productAttributeService.GetProductAttributesCombinationsAsync(product))
            .Select(attr => attr.HtmlClassify().ToUpperInvariant())
            .ToList();

        var variants = cleanForDisplay ? CleanVariantDictionary(part.Variants, allVariantsKeys) : part.Variants;
        var values = allVariantsKeys.ToDictionary(key => key, key => part[key]?.Value);
        var currencies = allVariantsKeys.ToDictionary(
            key => key,
            key => part[key]?.Currency.CurrencyIsoCode ?? _currencyOptions.Value.CurrentDisplayCurrency);

        model.InitializeVariants(variants, values, currencies);

        foreach (var key in model.VariantsCurrencies.Keys)
        {
            model.VariantsCurrencies[key] ??= _moneyService.DefaultCurrency.CurrencyIsoCode;
        }
    }

    private static Dictionary<string, T> CleanVariantDictionary<T>(
        IDictionary<string, T> variants,
        List<string> allVariantsKeys) =>
        variants
            .Where(pair => allVariantsKeys.Contains(pair.Key))
            .OrderBy(pair => allVariantsKeys.IndexOf(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
}
