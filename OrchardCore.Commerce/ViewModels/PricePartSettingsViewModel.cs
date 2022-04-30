using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Settings;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class PricePartSettingsViewModel
{
    public CurrencySelectionMode CurrencySelectionMode { get; set; }
    public string SpecificCurrencyIsoCode { get; set; }

    public IEnumerable<SelectListItem> CurrencySelectionModes { get; set; }

    public IEnumerable<SelectListItem> Currencies { get; set; }

    public CurrencySelectionMode SingleSelectionModeEditor { get; set; } = CurrencySelectionMode.SpecificCurrency;

    [BindNever]
    public PricePartSettings PricePartSettings { get; set; }
}
