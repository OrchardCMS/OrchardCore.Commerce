using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class ProductPartViewModel
    {
        public string Sku { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public ProductPart ProductPart { get; set; }

        [BindNever]
        public IEnumerable<ProductAttributeDescription> Attributes { get; set; }

        [BindNever]
        public bool CanBeBought { get; set; } = true;
    }
}
