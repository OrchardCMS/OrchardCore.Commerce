using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Promotion.ViewModels;

public class DiscountPartViewModel
{
    [BindNever]
    public DiscountInformation Discount { get; set; }

    [BindNever]
    public PriceField NewPrice { get; set; } = new();

    [BindNever]
    public ContentItem ContentItem { get; set; }

    [BindNever]
    public IList<string> OldPriceClassNames { get; } = new List<string>();
}
