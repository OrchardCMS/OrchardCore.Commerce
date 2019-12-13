using Money;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    public class PriceBookPrice
    {
        public IContent PriceBook { get; set; }
        public IContent PriceBookEntry { get; set; }
        public Amount Price { get; set; }
    }
}
