using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.ContentFields.Settings;
using OrchardCore.Commerce.ContentFields.ViewModels;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Settings;

public class PriceFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<PriceField>
{
    private readonly IStringLocalizer<PriceFieldSettingsDriver> T;
    private readonly IMoneyService _moneyService;

    public PriceFieldSettingsDriver(IStringLocalizer<PriceFieldSettingsDriver> localizer, IMoneyService moneyService)
    {
        T = localizer;
        _moneyService = moneyService;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition model, BuildEditorContext context) =>
        Initialize($"{nameof(PriceFieldSettings)}_Edit", (Action<PriceFieldSettingsEditViewModel>)(viewModel =>
            {
                var settings = model.GetSettings<PriceFieldSettings>();

                viewModel.Hint = settings.Hint;
                viewModel.Label = settings.Label;
                viewModel.Required = settings.Required;

                viewModel.CurrencySelectionMode = settings.CurrencySelectionMode;
                viewModel.CurrencySelectionModes = new SelectList(
                    new[]
                    {
                        new CurrencySelectionModeItem(CurrencySelectionMode.AllCurrencies, T["All Currencies"]),
                        new CurrencySelectionModeItem(CurrencySelectionMode.DefaultCurrency, T["Default Currency"]),
                        new CurrencySelectionModeItem(CurrencySelectionMode.SpecificCurrency, T["Specific Currency"]),
                    },
                    nameof(CurrencySelectionModeItem.Value),
                    nameof(CurrencySelectionModeItem.Text),
                    (int)settings.CurrencySelectionMode);

                viewModel.SpecificCurrencyIsoCode = settings.SpecificCurrencyIsoCode;
                viewModel.Currencies = _moneyService.GetSelectableCurrencies(
                    CurrencySelectionMode.AllCurrencies,
                    localizer: T,
                    currentValue: settings.SpecificCurrencyIsoCode);
            }))
            .PlaceInContent();

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition model, UpdatePartFieldEditorContext context)
    {
        var viewModel = await context.CreateModelAsync<PriceFieldSettingsEditViewModel>(Prefix);

        context.Builder.WithSettings(new PriceFieldSettings
        {
            Hint = viewModel.Hint,
            Label = viewModel.Label,
            Required = viewModel.Required,
            CurrencySelectionMode = viewModel.CurrencySelectionMode,
            SpecificCurrencyIsoCode = viewModel.CurrencySelectionMode == CurrencySelectionMode.SpecificCurrency
                ? viewModel.SpecificCurrencyIsoCode
                : null,
        });

        return await EditAsync(model, context);
    }

    private sealed record CurrencySelectionModeItem(int Value, string Text)
    {
        public CurrencySelectionModeItem(CurrencySelectionMode value, string text)
            : this((int)value, text)
        { }
    }
}
