using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace OrchardCore.Commerce.Indexes;

public class ProductPartIndex : MapIndex, ISkuHolder
{
    public string ContentItemId { get; set; }
    public string Sku { get; set; }
}

/// <summary>
/// Creates an index of content items (products in this case) by SKU.
/// </summary>
public class ProductPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<ProductPartIndex>()
            .Map(contentItem =>
                contentItem.IsPublished() &&
                contentItem.TryGet<ProductPart>(out var productPart) &&
                !string.IsNullOrEmpty(productPart.Sku)
                    ? new ProductPartIndex
                    {
                        Sku = productPart.Sku.ToUpperInvariant(),
                        ContentItemId = contentItem.ContentItemId,
                    }
                    : null);
}
