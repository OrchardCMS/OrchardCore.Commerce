using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakeProductService : IProductService
{
    public Task<IEnumerable<ProductPart>> GetProductsAsync(IEnumerable<string> skus) =>
        Task.FromResult(skus.Select(sku => new ProductPart
        {
            Sku = sku,
            ContentItem = new ContentItem { ContentType = "Product" },
        }));

    public string GetVariantKey(string sku)
    {
        var dashIndex = sku.IndexOf(value: "-", StringComparison.InvariantCulture);
        return dashIndex == -1 ? sku : sku[dashIndex..];
    }

    public async Task<(PriceVariantsPart Part, string VariantKey)> GetExactVariantAsync(string sku)
    {
        var productPart = await ProductServiceExtensions.GetProductAsync(this, sku);
        var priceVariantsPart = productPart.ContentItem.As<PriceVariantsPart>();

        return (priceVariantsPart, GetVariantKey(sku));
    }
}
