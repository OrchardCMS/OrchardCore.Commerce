using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions;

public interface IProductService
{
    Task<ProductPart> GetProductAsync(string sku);
    Task<IEnumerable<ProductPart>> GetProductsAsync(IEnumerable<string> skus);
    async Task<IDictionary<string, ProductPart>> GetProductDictionaryAsync(IEnumerable<string> skus)
        => (await GetProductsAsync(skus)).ToDictionary(product => product.Sku);
}
