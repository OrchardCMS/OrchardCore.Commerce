using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Extensions;

public static class ProductServiceExtensions
{
    public static async Task<ProductPart> GetProductAsync(this IProductService service, string sku) =>
        (await service.GetProductsAsync(new[] { sku })).SingleOrDefault();

    public static async Task<IDictionary<string, ProductPart>> GetProductDictionaryAsync(
        this IProductService service,
        IEnumerable<string> skus) =>
        (await service.GetProductsAsync(skus)).ToDictionary(product => product.Sku);
}
