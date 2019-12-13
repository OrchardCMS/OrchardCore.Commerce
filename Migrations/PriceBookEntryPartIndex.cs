using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using YesSql.Indexes;

namespace OrchardCore.Commerce.Indexes
{
    public class PriceBookEntryPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string PriceBookContentItemId { get; set; }
        public string ProductContentItemId { get; set; }
        public bool UseStandardPrice { get; set; }
        public decimal? AmountValue { get; set; }
        public string AmountCurrencyIsoCode { get; set; }
    }

    /// <summary>
    /// Creates an index of content items (price book entries in this case)
    /// </summary>
    public class PriceBookEntryPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<PriceBookEntryPartIndex>()
                .Map(contentItem =>
                {
                    if (contentItem.IsPublished())
                    {
                        var priceBookEntryPart = contentItem.As<PriceBookEntryPart>();
                        if (priceBookEntryPart != null)
                        {
                            var pricePart = contentItem.As<PricePart>();
                            if (pricePart != null)
                            {
                                var containedPart = contentItem.As<ContainedPart>();
                                if (containedPart != null)
                                {
                                    var priceBookEntryPartIndex = new PriceBookEntryPartIndex
                                    {
                                        ContentItemId = contentItem.ContentItemId,
                                        PriceBookContentItemId = containedPart.ListContentItemId,
                                        ProductContentItemId = priceBookEntryPart.ProductContentItemId,
                                        UseStandardPrice = priceBookEntryPart.UseStandardPrice
                                    };

                                    if (!priceBookEntryPart.UseStandardPrice && pricePart.Price != null)
                                    {
                                        priceBookEntryPartIndex.AmountValue = pricePart.Price.Value;
                                        priceBookEntryPartIndex.AmountCurrencyIsoCode = pricePart.Price.Currency.CurrencyIsoCode;
                                    }

                                    return priceBookEntryPartIndex;
                                }
                            }
                        }
                    }
                    return null;
                });
        }
    }
}