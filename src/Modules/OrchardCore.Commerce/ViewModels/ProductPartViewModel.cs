using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class ProductPartViewModel : ISkuHolderContent
{
    public string Sku { get; set; }

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public ProductPart ProductPart { get; set; }

    [BindNever]
    public IEnumerable<ProductAttributeDescription> Attributes { get; set; }

    [BindNever]
    public IDictionary<string, bool> CanBeBought { get; private set; } = new Dictionary<string, bool>();
}
