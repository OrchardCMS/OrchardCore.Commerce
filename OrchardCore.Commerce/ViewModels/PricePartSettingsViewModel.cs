using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.ViewModels;

public class PricePartSettingsViewModel
{
    public CurrencySelectionMode CurrencySelectionMode { get; set; }
    public string SpecificCurrencyIsoCode { get; set; }

    public IEnumerable<SelectListItem> CurrencySelectionModes { get; set; }

    public IEnumerable<SelectListItem> Currencies { get; set; }

    public CurrencySelectionMode SingleSelectionModeEditor => CurrencySelectionMode.SpecificCurrency;

    [BindNever]
    public PricePartSettings PricePartSettings { get; set; }
}
