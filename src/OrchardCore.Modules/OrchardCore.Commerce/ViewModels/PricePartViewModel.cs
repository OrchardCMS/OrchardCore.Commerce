using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Money;
using Money.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class PricePartViewModel
    {
        public decimal PriceValue { get; set; }
        public string PriceCurrency { get; set; }

        public IEnumerable<ICurrency> Currencies { get; set; }

        public ICurrency CurrentDisplayCurrency { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public PricePart PricePart { get; set; }

        [BindNever]
        public Amount Price { get; set; }
    }
}
