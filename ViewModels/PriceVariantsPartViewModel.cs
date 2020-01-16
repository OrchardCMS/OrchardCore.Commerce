using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Money;
using Money.Abstractions;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class PriceVariantsPartViewModel
    {
        public decimal BasePriceValue { get; set; }
        public string PriceCurrency { get; set; }
        public Dictionary<string, decimal> VariantsValues { get; set; }

        public IEnumerable<ICurrency> Currencies { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public PriceVariantsPart PriceVariantsPart { get; set; }

        [BindNever]
        public Amount BasePrice { get; set; }

        [BindNever]
        public Dictionary<string, Amount> Variants { get; set; }
    }
}
