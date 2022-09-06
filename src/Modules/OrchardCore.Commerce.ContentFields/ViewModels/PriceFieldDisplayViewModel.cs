using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.ContentFields.ViewModels;

public class PriceFieldDisplayViewModel
{
    public string AllowedIsoCode { get; set; }

    [BindNever]
    public PriceField Field { get; set; }

    [BindNever]
    public ContentPart Part { get; set; }

    [BindNever]
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }
}
