using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Settings;

namespace OrchardCore.Commerce.ViewModels
{
    public class PricePartSettingsViewModel
    {
        public string CurrencySelectionMode { get; set; }
        public string CurrencyIsoCode { get; set; }

        public IEnumerable<SelectListItem> CurrencySelectionModes { get; set; }

        public IEnumerable<SelectListItem> Currencies { get; set; }

        [BindNever]
        public PricePartSettings PricePartSettings { get; set; }
    }
}
