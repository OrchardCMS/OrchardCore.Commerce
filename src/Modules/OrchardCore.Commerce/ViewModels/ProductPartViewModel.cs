using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class ProductPartViewModel : ISkuHolderContent
{
    public string Sku { get; set; }

    [BindNever]
    public bool IsSkuReadOnly { get; set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public ProductPart ProductPart { get; set; }

    [BindNever]
    public IEnumerable<ProductAttributeDescription> Attributes { get; set; }

    [BindNever]
#pragma warning disable CA2227
    public IDictionary<string, bool> CanBeBought { get; set; } = new Dictionary<string, bool>();
#pragma warning restore CA2227
}
