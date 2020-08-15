using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Money;
using Money.Abstractions;

namespace OrchardCore.Commerce.ViewModels
{
    public class CommerceSettingsViewModel
    {
        public string DefaultCurrency { get; set; }
        public string CurrentDisplayCurrency { get; set; }

        public IEnumerable<SelectListItem> Currencies { get; set; }

        [BindNever]
        public Currency Currency { get; set; }
    }
}
