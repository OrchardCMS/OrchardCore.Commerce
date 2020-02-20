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
        public Dictionary<string, Amount> Variants { get; set; }
    }
}
