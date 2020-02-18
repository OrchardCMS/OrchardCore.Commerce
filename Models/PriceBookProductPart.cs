using Money;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// The price book product part shows related price books for a product
    /// </summary>
    public class PriceBookProductPart : ContentPart
    {
        // Temporary storage allowing the data to be persisted until after the Product is Created or Updated
        public IEnumerable<PriceBookEntry> TemporaryPriceBookEntries { get; set; }
    }

    public class PriceBookEntry
    {
        public string PriceBookContentItemId { get; set; }
        public string PriceBookEntryContentItemId { get; set; }

        // From PriceBookEntryPart
        public bool UseStandardPrice { get; set; }

        // From PricePart
        public Amount Price { get; set; } = new Amount(0, Currency.UnspecifiedCurrency);
    }
}
