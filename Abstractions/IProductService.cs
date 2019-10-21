using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductService
    {
        Task<ProductPart> GetProduct(string sku);
        Task<IEnumerable<ProductPart>> GetProducts(IEnumerable<string> skus);
        async Task<IDictionary<string, ProductPart>> GetProductDictionary(IEnumerable<string> skus)
            => (await GetProducts(skus)).ToDictionary(product => product.Sku);
    }
}
