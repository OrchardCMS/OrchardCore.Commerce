using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Money;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class PricePartViewModel
    {
        public decimal PriceValue { get; set; }
        public string PriceCurrency { get; set; }

        public IEnumerable<ICurrency> Currencies { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public PricePart PricePart { get; set; }

        [BindNever]
        public Amount Price { get; set; }
    }
}
