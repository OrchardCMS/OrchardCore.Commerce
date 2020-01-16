using System.Collections.Generic;
using Money;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// A product variants prices based on predefined attributes.
    /// </summary>
    public class PriceVariantsPart : ContentPart
    {
        public Amount BasePrice { get; set; }

        public Dictionary<string, decimal> Variants { get; set; }
    }
}
