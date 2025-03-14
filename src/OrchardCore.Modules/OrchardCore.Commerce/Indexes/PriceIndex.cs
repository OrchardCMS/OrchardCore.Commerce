using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Linq;
using YesSql.Indexes;

namespace OrchardCore.Commerce.Indexes;

public class PriceIndex : MapIndex
{
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}

/// <summary>
/// Creates an index of content items (products in this case) by SKU.
/// </summary>
public class PriceIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<PriceIndex>()
            .Map(contentItem =>
            {
                if (!contentItem.Published || !contentItem.Latest) return null;

                if (contentItem.As<PricePart>() is { Price.Value: var price })
                {
                    return new PriceIndex
                    {
                        MinPrice = price,
                        MaxPrice = price,
                    };
                }

                var variants = contentItem.As<PriceVariantsPart>()?.Variants;
                if (variants?.Any() == true)
                {
                    var amounts = variants.Values.Select(amount => amount.Value).ToList();
                    return new PriceIndex
                    {
                        MinPrice = amounts.Min(),
                        MaxPrice = amounts.Max(),
                    };
                }

                return null;
            });
}
