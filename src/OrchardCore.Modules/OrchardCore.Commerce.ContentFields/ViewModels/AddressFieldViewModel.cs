using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class AddressFieldViewModel
{
    public Address Address { get; set; }

    public string UserAddressToSave { get; set; }
    public bool ToBeSaved { get; set; }

    [BindNever]
    public HtmlString AddressHtml { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Regions { get; set; }

    [BindNever]
    public IDictionary<string, IDictionary<string, string>> Provinces { get; } =
        new Dictionary<string, IDictionary<string, string>>();

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public AddressField AddressPart { get; set; }

    [BindNever]
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }
}
