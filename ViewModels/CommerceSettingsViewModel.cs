using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Money;
using Money.Abstractions;

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
