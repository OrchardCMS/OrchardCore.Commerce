using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// A simple product price.
    /// </summary>
    public class PricePart : ContentPart, IPrice
    {
        public decimal Price { get; set; }
    }
}
