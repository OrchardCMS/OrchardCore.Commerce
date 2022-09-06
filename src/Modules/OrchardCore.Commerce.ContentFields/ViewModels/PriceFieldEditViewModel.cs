using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.ContentFields.ViewModels;

public class PriceFieldEditViewModel
{
    public decimal Value { get; set; }
    public string Currency { get; set; }

    [BindNever]
    public bool IsValid { get; set; }

    [BindNever]
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }

    [BindNever]
    public SelectList Currencies { get; set; }

    [BindNever]
    public PriceFieldSettings Settings { get; set; }

    [BindNever]
    public string PreferredCurrencyIsoCode { get; set; }
}
