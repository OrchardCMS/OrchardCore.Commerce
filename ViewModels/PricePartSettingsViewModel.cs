using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.ViewModels
{
    public class PricePartSettingsViewModel
    {
        public CurrencySelectionModeEnum CurrencySelectionMode { get; set; }
        public string SpecificCurrencyIsoCode { get; set; }

        public IEnumerable<SelectListItem> CurrencySelectionModes { get; set; }

        public IEnumerable<SelectListItem> Currencies { get; set; }

        public CurrencySelectionModeEnum SingleSelectionModeEditor => CurrencySelectionModeEnum.SpecificCurrency;

        [BindNever]
        public PricePartSettings PricePartSettings { get; set; }
    }
}
