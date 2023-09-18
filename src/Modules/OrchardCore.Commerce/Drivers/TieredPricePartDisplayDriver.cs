using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class TieredPricePartDisplayDriver : ContentPartDisplayDriver<TieredPricePart>
{
    private readonly IMoneyService _moneyService;
    private readonly IOptions<CurrencySettings> _currencyOptions;
    private readonly IStringLocalizer<TieredPricePartDisplayDriver> T;

    public TieredPricePartDisplayDriver(
        IMoneyService moneyService,
        IOptions<CurrencySettings> currencyOptions,
        IStringLocalizer<TieredPricePartDisplayDriver> localizer)
    {
        _moneyService = moneyService;
        _currencyOptions = currencyOptions;
        T = localizer;
    }

    public override IDisplayResult Display(TieredPricePart part, BuildPartDisplayContext context) =>
        Initialize<TieredPricePartViewModel>(GetDisplayShapeType(context), viewModel => BuildViewModel(viewModel, part))
            .Location("Detail", "Content:25")
            .Location("Summary", "Meta:10");

    public override IDisplayResult Edit(TieredPricePart part, BuildPartEditorContext context) =>
        Initialize<TieredPricePartViewModel>(GetEditorShapeType(context), viewModel =>
        {
            BuildViewModel(viewModel, part);
            viewModel.Currencies = _moneyService.Currencies;
        });

    public override async Task<IDisplayResult> UpdateAsync(
        TieredPricePart part,
        IUpdateModel updater,
        UpdatePartEditorContext context)
    {
        var viewModel = new TieredPricePartViewModel();
        if (await updater.TryUpdateModelAsync(
            viewModel,
            Prefix,
            viewModel => viewModel.TieredValuesSerialized,
            viewModel => viewModel.Currency))
        {
            IEnumerable<PriceTier> tieredValues = Enumerable.Empty<PriceTier>();
            try
            {
                viewModel.SortTiersByQuantity();
                tieredValues = viewModel.GetTieredValues();
            }
            catch (JsonException)
            {
                updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.TieredValuesSerialized),
                    T["The given tiered prices are not valid."]);
            }

            // Restoring tiers so that only the new values are stored.
            part.TieredPrices.RemoveAll();

            if (tieredValues.Any(tier => tier.UnitPrice is null))
            {
                updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.TieredValuesSerialized),
                    T["You need to set a price for every tier."]);
            }

            if (tieredValues
                .GroupBy(tier => tier.Quantity)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .Any())
            {
                updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.TieredValuesSerialized),
                    T["There are duplicate tiers."]);
            }

            if (tieredValues.Any(tier => tier.Quantity < 1))
            {
                updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.TieredValuesSerialized),
                    T["You need to set a quantity greater than 0 for every tier."]);
            }

            if (tieredValues.Any(tier => tier.UnitPrice <= 0))
            {
                updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.TieredValuesSerialized),
                    T["You need to set a unit price greater than 0 for every tier."]);
            }

            foreach (var tieredValue in tieredValues)
            {
                part.TieredPrices[tieredValue.Quantity] = _moneyService.Create(
                    tieredValue.UnitPrice.Value,
                    viewModel.Currency);
            }

        }

        return await EditAsync(part, context);
    }

    private void BuildViewModel(TieredPricePartViewModel model, TieredPricePart part)
    {
        model.ContentItem = part.ContentItem;
        model.TieredPricePart = part;

        var tieredPrices = part.TieredPrices ?? new Dictionary<int, Amount>();

        model.InitializeTiers(
            tieredPrices,
            part.TieredPrices.Any()
                ? part.TieredPrices.FirstOrDefault().Value.Currency.CurrencyIsoCode
                : _currencyOptions.Value.CurrentDisplayCurrency);

        model.SortTiersByQuantity();
    }
}
