using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// A part used to determine appropriate price book based on user
    /// </summary>
    public class PriceBookByUserPart : ContentPart
    {
        public string UserName { get; set; }
        public string PriceBookContentItemId { get; set; }
    }
}
