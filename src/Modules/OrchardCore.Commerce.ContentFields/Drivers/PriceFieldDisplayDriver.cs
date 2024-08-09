using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.ContentFields.Settings;
using OrchardCore.Commerce.ContentFields.ViewModels;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.ContentFields.Drivers;

public class PriceFieldDisplayDriver : ContentFieldDisplayDriver<PriceField>
{
    private readonly IMoneyService _moneyService;

    internal readonly IStringLocalizer T;

    public PriceFieldDisplayDriver(IMoneyService moneyService, IStringLocalizer<PriceFieldDisplayDriver> localizer)
    {
        _moneyService = moneyService;
        T = localizer;
    }

    public override IDisplayResult Display(PriceField field, BuildFieldDisplayContext fieldDisplayContext) =>
        field.Amount.IsValid
            ? Initialize<PriceFieldDisplayViewModel>(GetDisplayShapeType(fieldDisplayContext), model =>
                {
                    model.Field = field;
                    model.Part = fieldDisplayContext.ContentPart;
                    model.PartFieldDefinition = fieldDisplayContext.PartFieldDefinition;

                    var settings = fieldDisplayContext.PartFieldDefinition.GetSettings<PriceFieldSettings>();
                    model.AllowedIsoCode = settings.CurrencySelectionMode switch
                    {
                        CurrencySelectionMode.AllCurrencies => null,
                        CurrencySelectionMode.DefaultCurrency => _moneyService.DefaultCurrency.CurrencyIsoCode,
                        CurrencySelectionMode.SpecificCurrency => settings.SpecificCurrencyIsoCode,
                        _ => throw new InvalidOperationException(
                            $"Unknown {nameof(CurrencySelectionMode)} value \"{settings.CurrencySelectionMode}\"."),
                    };
                })
                .Location(CommonContentDisplayTypes.Detail, CommonLocationNames.Content)
                .Location(CommonContentDisplayTypes.Summary, CommonLocationNames.Content)
            : null;

    public override IDisplayResult Edit(PriceField field, BuildFieldEditorContext context) =>
        Initialize<PriceFieldEditViewModel>(GetEditorShapeType(context), model =>
        {
            var settings = context.PartFieldDefinition.GetSettings<PriceFieldSettings>();
            model.IsValid = field.Amount.IsValid;
            model.PreferredCurrencyIsoCode = settings.CurrencySelectionMode == CurrencySelectionMode.SpecificCurrency
                ? settings.SpecificCurrencyIsoCode
                : _moneyService.DefaultCurrency.CurrencyIsoCode;

            if (model.IsValid)
            {
                model.Value = field.Amount.Value;
                model.Currency = field.Amount.Currency.CurrencyIsoCode;
            }
            else
            {
                model.Value = 0;
                model.Currency = model.PreferredCurrencyIsoCode;
            }

            model.PartFieldDefinition = context.PartFieldDefinition;
            model.Settings = settings;
            model.Currencies = _moneyService.GetSelectableCurrencies(
                model.Settings.CurrencySelectionMode,
                model.Settings.SpecificCurrencyIsoCode);
        });

    public override async Task<IDisplayResult> UpdateAsync(
        PriceField field,
        UpdateFieldEditorContext context)
    {
        var viewModel = await context.CreateModelAsync<PriceFieldEditViewModel>(Prefix);

        var settings = context.PartFieldDefinition.GetSettings<PriceFieldSettings>();
        var isInvalid = IsCurrencyInvalid(viewModel.Currency);

        if (isInvalid && settings.Required)
        {
            var label = string.IsNullOrEmpty(settings.Label)
                ? context.PartFieldDefinition.DisplayName()
                : settings.Label;
            context.AddModelError(
                nameof(viewModel.Currency),
                T["The field {0} is invalid.", label]);
        }

        field.Amount = isInvalid
                ? Amount.Unspecified
                : _moneyService.Create(viewModel.Value, viewModel.Currency);

        return await EditAsync(field, context);
    }

    private static bool IsCurrencyInvalid(string isoCode) =>
        string.IsNullOrEmpty(isoCode) || isoCode == Currency.UnspecifiedCurrency.CurrencyIsoCode;
}
