using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class CommerceSettingsViewModel
{
    public string DefaultCurrency { get; set; }
    public string CurrentDisplayCurrency { get; set; }

    public IEnumerable<SelectListItem> Currencies { get; set; }

    [BindNever]
    public Currency Currency { get; set; }
}
