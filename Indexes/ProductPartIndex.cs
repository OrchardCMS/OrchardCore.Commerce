using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace OrchardCore.Commerce.Indexes
{
    public class ProductPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string Sku { get; set; }
    }

    /// <summary>
    /// Creates an index of content items (products in this case) by SKU.
    /// </summary>
    public class ProductPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<ProductPartIndex>()
                .Map(contentItem =>
                {
                    if (!contentItem.IsPublished())
                    {
                        return null;
                    }

                    var productPart = contentItem.As<ProductPart>();

                    if (productPart?.Sku == null)
                    {
                        return null;
                    }

                    return new ProductPartIndex
                    {
                        Sku = productPart.Sku.ToLowerInvariant(),
                        ContentItemId = contentItem.ContentItemId,
                    };
                });
        }
    }
}