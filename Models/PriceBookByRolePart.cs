using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// A part used to determine appropriate price book based on role
    /// </summary>
    public class PriceBookByRolePart : ContentPart
    {
        public string RoleName { get; set; }
        public string PriceBookContentItemId { get; set; }
    }
}
