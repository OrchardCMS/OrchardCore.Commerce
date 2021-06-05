using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Settings
{
    public class PricePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IStringLocalizer<PricePartSettingsDisplayDriver> S;
        private readonly IMoneyService _moneyService;

        public PricePartSettingsDisplayDriver(IStringLocalizer<PricePartSettingsDisplayDriver> localizer, IMoneyService moneyService)
        {
            S = localizer;
            _moneyService = moneyService;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(PricePart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            return Initialize<PricePartSettingsViewModel>("PricePartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<PricePartSettings>();

                model.CurrencySelectionMode = settings.CurrencySelectionMode;
                model.CurrencySelectionModes = new List<SelectListItem>()
                {
                    new SelectListItem(CurrencySelectionModeEnum.AllCurrencies.ToString(), S["All Currencies"]),
                    new SelectListItem(CurrencySelectionModeEnum.DefaultCurrency.ToString(), S["Default Currency"]),
                    new SelectListItem(CurrencySelectionModeEnum.SpecificCurrency.ToString(), S["Specific Currency"])
                };
                model.SpecificCurrencyIsoCode = settings.SpecificCurrencyIsoCode;
                model.Currencies = _moneyService.Currencies
                        .OrderBy(c => c.CurrencyIsoCode)
                        .Select(c => new SelectListItem(
                            c.CurrencyIsoCode,
                            $"{c.CurrencyIsoCode} {c.Symbol} - {S[c.EnglishName]}"));
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(PricePart), contentTypePartDefinition.PartDefinition.Name))
            {
                return null;
            }

            var model = new PricePartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.CurrencySelectionMode,
                m => m.SpecificCurrencyIsoCode);

            context.Builder.WithSettings(new PricePartSettings
            {
                CurrencySelectionMode = model.CurrencySelectionMode,
                SpecificCurrencyIsoCode =
                    model.CurrencySelectionMode == CurrencySelectionModeEnum.SpecificCurrency
                        ? model.SpecificCurrencyIsoCode : null
            });

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
