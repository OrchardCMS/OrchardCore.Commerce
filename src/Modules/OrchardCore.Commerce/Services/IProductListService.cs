using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// Provides a way to get the products for a product list.
/// </summary>
public interface IProductListService
{
    /// <summary>
    /// Gets the products for the given product list and filter parameters.
    /// </summary>
    Task<ProductList> GetProductsAsync(ProductListPart productList, ProductListFilterParameters filterParameters);

    /// <summary>
    /// Gets the order by options for the given product list.
    /// </summary>
    Task<IEnumerable<string>> GetOrderByOptionsAsync(ProductListPart productList);

    /// <summary>
    /// Gets the filter IDs for the given product list.
    /// </summary>
    Task<IEnumerable<string>> GetFilterIdsAsync(ProductListPart productList);
}
