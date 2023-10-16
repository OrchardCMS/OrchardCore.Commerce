using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public interface IProductListItemProvider
{
    int Order { get; }
    Task<bool> CanHandleAsync(ProductListPart productList);
    Task<IQuery<ContentItem>> BuildQueryAsync(ProductListQueryContext context);
    Task<IEnumerable<ContentItem>> PostProcessListAsync(ProductListQueryContext context);
}
