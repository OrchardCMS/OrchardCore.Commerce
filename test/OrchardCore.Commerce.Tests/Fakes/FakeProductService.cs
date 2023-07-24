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

    // IProductService's method needs to be created, but implementation is unnecessary as the tests do not use it.
    public Task<(PriceVariantsPart Part, string VariantKey)> GetExactVariantAsync(string sku) => throw new NotSupportedException();

    // IProductService's method needs to be created, but implementation is unnecessary as the tests do not use it.
    public Task<IEnumerable<ProductPart>> GetProductsByContentItemVersionsAsync(IEnumerable<string> contentItemVersions) =>
        throw new NotSupportedException();

    // IProductService's method needs to be created, but implementation is unnecessary as the tests do not use it.
    public string GetVariantKey(string sku) => throw new NotSupportedException();

    // IProductService's method needs to be created, but implementation is unnecessary as the tests do not use it.
    public string GetOrderFullSku(ShoppingCartItem item, ProductPart productPart) => throw new NotSupportedException();
}
