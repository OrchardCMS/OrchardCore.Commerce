using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static Lombiq.HelpfulLibraries.OrchardCore.Contents.CommonContentDisplayTypes;

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
            .Location(Detail, "Content:25")
            .Location(Summary, "Meta:10");

    public override IDisplayResult Edit(TieredPricePart part, BuildPartEditorContext context) =>
        Initialize<TieredPricePartViewModel>(GetEditorShapeType(context), viewModel =>
        {
            BuildViewModel(viewModel, part);
            viewModel.Currencies = _moneyService.Currencies;
        });

    public override async Task<IDisplayResult> UpdateAsync(
        TieredPricePart part,
        UpdatePartEditorContext context)
    {
        var viewModel = new TieredPricePartViewModel();
        if (await context.Updater.TryUpdateModelAsync(
            viewModel,
            Prefix,
            viewModel => viewModel.DefaultPrice,
            viewModel => viewModel.TieredValuesSerialized,
            viewModel => viewModel.Currency))
        {
            var priceTiers = Array.Empty<PriceTier>();
            try
            {
                priceTiers = viewModel.DeserializePriceTiers().ToArray();
            }
            catch (JsonException)
            {
                context.Updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.TieredValuesSerialized),
                    T["The given tiered prices are not valid."]);
            }

            // Restoring tiers so that only the new values are stored.
            part.PriceTiers.RemoveAll();

            if (priceTiers.Exists(tier => tier.UnitPrice is null))
            {
                context.Updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.TieredValuesSerialized),
                    T["You need to set a price for every tier."]);
            }

            if (viewModel.DefaultPrice == null || viewModel.DefaultPrice.Value <= 0)
            {
                context.Updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.DefaultPrice),
                    T["You need to set a default price greater than 0."]);
            }

            if (priceTiers
                .GroupBy(tier => tier.Quantity)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .Any())
            {
                context.Updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.TieredValuesSerialized),
                    T["There are duplicate tiers."]);
            }

            if (priceTiers.Exists(tier => tier.UnitPrice < 0))
            {
                context.Updater.ModelState.AddModelError(
                    nameof(TieredPricePartViewModel.TieredValuesSerialized),
                    T["You need to set a unit price greater or equal to 0 for every tier."]);
            }

            part.PriceTiers.AddRange(priceTiers);

            part.DefaultPrice = _moneyService.Create(
                viewModel.DefaultPrice ?? 0,
                viewModel.Currency);
        }

        return await EditAsync(part, context);
    }

    private void BuildViewModel(TieredPricePartViewModel model, TieredPricePart part)
    {
        model.ContentItem = part.ContentItem;
        model.TieredPricePart = part;

        var priceTiers = part.PriceTiers;

        model.InitializeTiers(
            part.DefaultPrice,
            priceTiers,
            part.DefaultPrice.Currency.CurrencyIsoCode ?? _currencyOptions.Value.CurrentDisplayCurrency);
    }
}
