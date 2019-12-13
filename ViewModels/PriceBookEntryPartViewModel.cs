using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.ViewModels
{
    public class PriceBookEntryPartViewModel
    {
        public string ProductContentItemId { get; set; }
        public bool UseStandardPrice { get; set; }

        public bool StandardPriceBook { get; set; }
        public IContent Product { get; set; }
    }
}
