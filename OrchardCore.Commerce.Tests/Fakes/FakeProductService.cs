using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakeProductService : IProductService
{
    public Task<ProductPart> GetProductAsync(string sku)
        => Task.FromResult(new ProductPart
        {
            Sku = sku,
            ContentItem = new ContentManagement.ContentItem { ContentType = "Product" },
        });

    public Task<IEnumerable<ProductPart>> GetProductsAsync(IEnumerable<string> skus)
        => Task.FromResult(skus.Select(sku => GetProductAsync(sku).Result));
}
