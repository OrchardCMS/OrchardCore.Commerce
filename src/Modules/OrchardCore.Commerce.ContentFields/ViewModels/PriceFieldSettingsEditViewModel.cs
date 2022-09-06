using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.ContentFields.Settings;

namespace OrchardCore.Commerce.ContentFields.ViewModels;

public class PriceFieldSettingsEditViewModel
{
    public string Hint { get; set; }
    public string Label { get; set; }
    public bool Required { get; set; }

    public CurrencySelectionMode CurrencySelectionMode { get; set; }
    public string SpecificCurrencyIsoCode { get; set; }

    [BindNever]
    public SelectList CurrencySelectionModes { get; set; }

    [BindNever]
    public PriceFieldSettings Settings { get; set; }

    [BindNever]
    public SelectList Currencies { get; set; }
}
