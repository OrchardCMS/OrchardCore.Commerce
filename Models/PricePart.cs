using System.Collections.Generic;
using Money;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// A simple product price.
    /// </summary>
    public class PricePart : ContentPart
    {
        public Amount Price { get; set; }

        public Dictionary<string, decimal> VariantsPrices { get; set; }
    }
}
