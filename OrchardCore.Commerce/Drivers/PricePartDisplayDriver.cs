using System.Collections.Generic;
using System.Threading.Tasks;
using Money;
using Money.Abstractions;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Drivers;

public class PricePartDisplayDriver : ContentPartDisplayDriver<PricePart>
{
    private readonly IMoneyService _moneyService;

    public PricePartDisplayDriver(IMoneyService moneyService) => _moneyService = moneyService;

    public override IDisplayResult Display(PricePart part, BuildPartDisplayContext context) =>
        Initialize<PricePartViewModel>(GetDisplayShapeType(context), m => BuildViewModelAsync(m, part))
            .Location("Detail", "Content:25")
            .Location("Summary", "Meta:10");

    public override IDisplayResult Edit(PricePart part, BuildPartEditorContext context)
    {
        var pricePartSettings = context.TypePartDefinition.GetSettings<PricePartSettings>();

        return Initialize<PricePartViewModel>(GetEditorShapeType(context), async m =>
        {
            await BuildViewModelAsync(m, part);

            // This is only required for the editor. Not the frontend display.
            m.Currencies = GetCurrencySelectionList(pricePartSettings);
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(PricePart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var updateModel = new PricePartViewModel();
        if (await updater.TryUpdateModelAsync(updateModel, Prefix, t => t.PriceValue, t => t.PriceCurrency))
        {
            part.Price = _moneyService.Create(updateModel.PriceValue, updateModel.PriceCurrency);
        }

        return await EditAsync(part, context);
    }

    private ValueTask BuildViewModelAsync(PricePartViewModel model, PricePart part)
    {
        model.ContentItem = part.ContentItem;

        model.Price = part.Price;
        model.PriceValue = part.Price.Value;
        model.PriceCurrency = part.Price.Currency.Equals(Currency.UnspecifiedCurrency)
            ? _moneyService.DefaultCurrency.CurrencyIsoCode
            : part.Price.Currency.CurrencyIsoCode;
        model.PricePart = part;
        model.CurrentDisplayCurrency = _moneyService.CurrentDisplayCurrency;

        return default;
    }

    private IEnumerable<ICurrency> GetCurrencySelectionList(PricePartSettings pricePartSettings)
    {
        var currencySelectionList = pricePartSettings.CurrencySelectionMode switch
        {
            CurrencySelectionMode.DefaultCurrency => new List<ICurrency> { _moneyService.DefaultCurrency },
            CurrencySelectionMode.SpecificCurrency => new List<ICurrency>
            {
                _moneyService.GetCurrency(pricePartSettings.SpecificCurrencyIsoCode),
            },
            CurrencySelectionMode.AllCurrencies => _moneyService.Currencies,
            _ => _moneyService.Currencies, // As a fallback show all currencies.
        };
        return currencySelectionList;
    }
}
