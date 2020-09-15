using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Money;
using Money.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class PriceVariantsPartViewModel
    {
        public Dictionary<string, decimal?> VariantsValues { get; set; }
        public Dictionary<string, string> VariantsCurrencies { get; set; }

        public IEnumerable<ICurrency> Currencies { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public PriceVariantsPart PriceVariantsPart { get; set; }

        [BindNever]
        public Dictionary<string, Amount> Variants { get; set; }
    }
}
