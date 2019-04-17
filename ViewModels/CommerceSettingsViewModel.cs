using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Money;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels
{
    public class CommerceSettingsViewModel
    {
        public string DefaultCurrency { get; set; }
        public IEnumerable<ICurrency> Currencies { get; set; }

        [BindNever]
        public Currency Currency { get; set; }
    }
}
