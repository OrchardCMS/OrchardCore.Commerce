using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductService
    {
        Task<ProductPart> GetProduct(string sku);
        Task<IEnumerable<ProductPart>> GetProducts(IEnumerable<string> skus);
    }
}
