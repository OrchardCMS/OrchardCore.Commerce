using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class PricePartViewModel
    {
        public decimal Price { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public PricePart PricePart { get; set; }
    }
}
