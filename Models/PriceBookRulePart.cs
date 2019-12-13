using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// The price book rule part categorizes a content item as a price book rule
    /// </summary>
    public class PriceBookRulePart : ContentPart
    {
        // Bigger number has more "weight" and is more important
        public decimal? Weight { get; set; }
    }
}
