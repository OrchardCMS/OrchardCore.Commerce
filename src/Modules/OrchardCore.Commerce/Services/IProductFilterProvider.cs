using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// Provides a way to filter a product list.
/// </summary>
public interface IProductFilterProvider
{
    /// <summary>
    /// The order in which the filter providers are applied.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Whether this provider can handle the given product list. If not, the next provider will be tried and this
    /// provider will be skipped.
    /// </summary>
    Task<bool> CanHandleAsync(ProductListPart productList);

    /// <summary>
    /// The IDs of the order by options that this provider can handle.
    /// </summary>
    Task<IEnumerable<string>> GetOrderByOptionIdsAsync(ProductListPart productList);

    /// <summary>
    /// The IDs of the filters that this provider can handle.
    /// </summary>
    Task<IEnumerable<string>> GetFilterIdsAsync(ProductListPart productListPart);

    /// <summary>
    /// Builds the query for the given product list and filter parameters.
    /// </summary>
    Task<IQuery<ContentItem>> BuildQueryAsync(ProductListFilterContext context);
}