using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.ContentFields.Settings;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ContentFields.ViewModels;

public class PriceFieldDisplayViewModel
{
    public IEnumerable<ICurrency> Currencies { get; set; }

    public ICurrency CurrentDisplayCurrency { get; set; }

    [BindNever]
    public PriceField Field { get; set; }

    [BindNever]
    public ContentPart Part { get; set; }

    [BindNever]
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }

    [BindNever]
    public PriceFieldSettings PriceFieldSettings { get; set; }
}
