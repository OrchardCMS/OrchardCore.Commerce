using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.ContentFields.Models;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Promotion.ViewModels;

public class DiscountPartViewModel
{
    [BindNever]
    public DiscountPart DiscountPart { get; set; }

    [BindNever]
    public PriceField NewPrice { get; set; } = new();

    [BindNever]
    public ContentItem ContentItem { get; set; }
}
