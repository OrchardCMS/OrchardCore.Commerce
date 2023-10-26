using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public interface IProductFilterProvider
{
    int Order { get; }
    Task<bool> CanHandleAsync(ProductListPart productList);
    Task<IEnumerable<string>> GetOrderByOptionIdsAsync(ProductListPart productList);
    Task<IEnumerable<string>> GetFilterOptionIdsAsync(ProductListPart productListPart);
    Task<IQuery<ContentItem>> BuildQueryAsync(ProductListFilterContext context);
}