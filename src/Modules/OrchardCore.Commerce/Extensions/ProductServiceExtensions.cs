using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

public static class ProductServiceExtensions
{
    public static async Task<ProductPart> GetProductAsync(this IProductService service, string sku) =>
        string.IsNullOrEmpty(sku) ? null : (await service.GetProductsAsync(new[] { sku })).SingleOrDefault();

    public static async Task<IDictionary<string, ProductPart>> GetProductDictionaryAsync(
        this IProductService service,
        IEnumerable<string> skus) =>
        (await service.GetProductsAsync(skus.WhereNot(string.IsNullOrWhiteSpace))).ToDictionary(product => product.Sku);

    public static async Task<IDictionary<string, ProductPart>> GetProductDictionaryByContentItemVersionsAsync(
        this IProductService service,
        IEnumerable<string> contentItemVersions) =>
        (await service.GetProductsByContentItemVersionsAsync(contentItemVersions.Distinct())).ToDictionary(product => product.Sku);
}
