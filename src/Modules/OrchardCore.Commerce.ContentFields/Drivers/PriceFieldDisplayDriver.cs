using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.ContentFields.Settings;
using OrchardCore.Commerce.ContentFields.ViewModels;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.ContentFields.Drivers;

public class PriceFieldDisplayDriver : ContentFieldDisplayDriver<PriceField>
{
    private readonly IMoneyService _moneyService;

    public PriceFieldDisplayDriver(IMoneyService moneyService) =>
        _moneyService = moneyService;

    public override IDisplayResult Display(PriceField field, BuildFieldDisplayContext fieldDisplayContext) =>
        Initialize<PriceFieldDisplayViewModel>(GetDisplayShapeType(fieldDisplayContext), model =>
            {
                model.Field = field;
                model.Part = fieldDisplayContext.ContentPart;
                model.PartFieldDefinition = fieldDisplayContext.PartFieldDefinition;

                var settings = fieldDisplayContext.PartFieldDefinition.GetSettings<PriceFieldSettings>();

                if (IsCurrencyInvalid(field.Amount?.Currency?.CurrencyIsoCode))
                {
                    field.Amount = settings.Required ? _moneyService.Create(0) : null;
                }
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");

    public override IDisplayResult Edit(PriceField field, BuildFieldEditorContext context) =>
        Initialize<PriceFieldEditViewModel>(GetEditorShapeType(context), model =>
        {
            model.Value = field.Amount?.Value ?? default;
            model.Currency = field.Amount?.Currency.CurrencyIsoCode;

            model.PartFieldDefinition = context.PartFieldDefinition;
            model.Settings = context.PartFieldDefinition.GetSettings<PriceFieldSettings>();
            model.Currencies = _moneyService.GetSelectableCurrencies(
                model.Settings.CurrencySelectionMode,
                model.Settings.SpecificCurrencyIsoCode);
        });

    public override async Task<IDisplayResult> UpdateAsync(
        PriceField field,
        IUpdateModel updater,
        UpdateFieldEditorContext context)
    {
        var updateModel = new PriceFieldEditViewModel();
        if (await updater.TryUpdateModelAsync(updateModel, Prefix))
        {
            field.Amount = IsCurrencyInvalid(updateModel.Currency)
                ? null
                : _moneyService.Create(updateModel.Value, updateModel.Currency);
        }

        return await EditAsync(field, context);
    }

    private static bool IsCurrencyInvalid(string isoCode) =>
        string.IsNullOrEmpty(isoCode) || isoCode == Currency.UnspecifiedCurrency.CurrencyIsoCode;
}
