using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    public class CurrencyPart : ContentPart
    {
        public string Name { get; set; }
        public string IsoCode { get; set; }
        public string Symbol { get; set; }
        public int DecimalPlaces { get; set; }
    }
}
