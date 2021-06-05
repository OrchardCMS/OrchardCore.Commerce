using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using InternationalAddress;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.ViewModels
{
    public class AddressFieldViewModel
    {
        public Address Address { get; set; }

        [BindNever]
        public HtmlString AddressHtml { get; set; }

        [BindNever]
        public IList<RegionInfo> Regions { get; set; }

        [BindNever]
        public IDictionary<string, IDictionary<string, string>> Provinces { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public AddressField AddressPart { get; set; }

        [BindNever]
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
