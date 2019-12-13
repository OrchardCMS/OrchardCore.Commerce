using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// The price book part describes the unique elements needed for a price book
    /// Utilize a recipe to add in a title part, a list part, and (optionally) a description
    /// </summary>
    public class PriceBookPart : ContentPart
    {
        public bool StandardPriceBook { get; set; }
    }
}
