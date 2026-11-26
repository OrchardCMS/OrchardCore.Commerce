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
        Initialize<PriceVariantsPartViewModel>(GetDisplayShapeType(context), async viewModel => await BuildViewModelAsync(viewModel, part))
            .Location("Detail", "Content:25")
            .Location("Summary", "Meta:10");

    public override IDisplayResult Edit(PriceVariantsPart part, BuildPartEditorContext context) =>
        Initialize<PriceVariantsPartViewModel>(GetEditorShapeType(context), async viewModel =>
        {
            await BuildViewModelAsync(viewModel, part);
            viewModel.Currencies = _moneyService.Currencies;

            foreach (var key in viewModel.VariantsCurrencies.Keys)
            {
                viewModel.VariantsCurrencies[key] ??= _moneyService.DefaultCurrency.CurrencyIsoCode;
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

        foreach ((var key, var value) in viewModel.VariantsValues)
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
        var product = part.ContentItem;
        var hasInventoryPart = product.TryGet<InventoryPart>(out var inventoryPart);
        if (hasInventoryPart)
        {
            await _productAttributeService.UpdateInventoryPartFromProductAttributesAsync(product);
        }

        model.ContentItem = product;
        model.PriceVariantsPart = part;

        var allVariantsKeys = (await _productAttributeService.GetProductAttributesCombinationsAsync(product))
            .Select(attr => attr.HtmlClassify().ToUpperInvariant())
            .ToList();

        var variants = part.Variants ?? new Dictionary<string, Amount>();
        var values = allVariantsKeys.ToDictionary(key => key, key => part[key]?.Value);
        var currencies = allVariantsKeys.ToDictionary(
            key => key,
            key => part[key]?.Currency.CurrencyIsoCode ?? _currencyOptions.Value.CurrentDisplayCurrency);

        model.InitializeVariants(variants, values, currencies);

        // When creating a new PriceVariantsProduct item, initialize default inventories.
        if (hasInventoryPart && !inventoryPart.Inventory.Any())
        {
            foreach (var variantKey in allVariantsKeys)
            {
                inventoryPart.Inventory.Add(variantKey, 0);
                inventoryPart.InventoryKeys.Add(variantKey);
            }
        }
    }
}
