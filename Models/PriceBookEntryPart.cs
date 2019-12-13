using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// The price book entry part describes the unique elements needed for a price book entry
    /// Utilize a recipe to add in a price part
    /// </summary>
    public class PriceBookEntryPart : ContentPart
    {
        public string ProductContentItemId { get; set; }
        public bool UseStandardPrice { get; set; }
    }
}
