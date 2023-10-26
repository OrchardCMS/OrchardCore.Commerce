using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public interface IProductListService
{
    Task<ProductList> GetProductsAsync(ProductListPart productList, ProductListFilterParameters filterParameters);
    Task<IEnumerable<string>> GetOrderByOptionsAsync(ProductListPart productList);
    Task<IEnumerable<string>> GetFilterIdsAsync(ProductListPart productList);
}
