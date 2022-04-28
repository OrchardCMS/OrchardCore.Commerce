using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Settings;

public class PricePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    private readonly IStringLocalizer<PricePartSettingsDisplayDriver> _s;
    private readonly IMoneyService _moneyService;

    public PricePartSettingsDisplayDriver(IStringLocalizer<PricePartSettingsDisplayDriver> localizer, IMoneyService moneyService)
    {
        _s = localizer;
        _moneyService = moneyService;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
    {
        if (model.PartDefinition.Name != nameof(PricePart)) return null;

        return Initialize("PricePartSettings_Edit", (Action<PricePartSettingsViewModel>)(viewModel =>
        {
            var settings = model.GetSettings<PricePartSettings>();

            viewModel.CurrencySelectionMode = settings.CurrencySelectionMode;
            viewModel.CurrencySelectionModes = new List<SelectListItem>
            {
                new(CurrencySelectionMode.AllCurrencies.ToString(), _s["All Currencies"]),
                new(CurrencySelectionMode.DefaultCurrency.ToString(), _s["Default Currency"]),
                new(CurrencySelectionMode.SpecificCurrency.ToString(), _s["Specific Currency"]),
            };
            viewModel.SpecificCurrencyIsoCode = settings.SpecificCurrencyIsoCode;
            viewModel.Currencies = _moneyService.Currencies
                .OrderBy(currency => currency.CurrencyIsoCode)
                .Select(currency => new SelectListItem(
                    currency.CurrencyIsoCode,
                    $"{currency.CurrencyIsoCode} {currency.Symbol} - {_s[currency.EnglishName]}"));
        }))
            .Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition model, UpdateTypePartEditorContext context)
    {
        if (model.PartDefinition.Name != nameof(PricePart)) return null;

        var viewModel = new PricePartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(
            viewModel,
            Prefix,
            m => m.CurrencySelectionMode,
            m => m.SpecificCurrencyIsoCode);

        context.Builder.WithSettings(new PricePartSettings
        {
            CurrencySelectionMode = viewModel.CurrencySelectionMode,
            SpecificCurrencyIsoCode = viewModel.CurrencySelectionMode == CurrencySelectionMode.SpecificCurrency
                    ? viewModel.SpecificCurrencyIsoCode
                    : null,
        });

        return await EditAsync(model, context.Updater);
    }
}
