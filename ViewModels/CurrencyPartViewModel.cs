using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class CurrencyPartViewModel
    {
        public string Name { get; set; }
        public string IsoCode { get; set; }
        public string Symbol { get; set; }
        public int DecimalPlaces { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public CurrencyPart CurrencyPart { get; set; }
    }
}
