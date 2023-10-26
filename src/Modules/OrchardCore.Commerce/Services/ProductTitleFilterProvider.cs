using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public class ProductTitleFilterProvider : IProductFilterProvider
{
    public const string TitleFilterId = "Title";
    public const string TitleAscOrderById = "TitleAsc";
    public const string TitleDescOrderById = "TitleDesc";

    public int Order { get; } = 10;

    public Task<bool> CanHandleAsync(ProductListPart productList) => Task.FromResult(true);

    public Task<IEnumerable<string>> GetOrderByOptionIdsAsync(ProductListPart productList) => Task.FromResult<IEnumerable<string>>(new[] { TitleAscOrderById, TitleDescOrderById });

    public Task<IEnumerable<string>> GetFilterOptionIdsAsync(ProductListPart productListPart) => Task.FromResult<IEnumerable<string>>(new[] { TitleFilterId });

    public Task<IQuery<ContentItem>> BuildQueryAsync(ProductListFilterContext context)
    {
        var query = context.Query;
        if (context.FilterParameters.FilterValues.TryGetValue(TitleFilterId, out var title))
        {
            query = query.With<ContentItemIndex>(index => index.DisplayText.Contains(title));
        }

        if (context.FilterParameters.OrderBy.Contains(TitleAscOrderById))
        {
            query = query.With<ContentItemIndex>().OrderBy(index => index.DisplayText);
        }
        else if (context.FilterParameters.OrderBy.Contains(TitleDescOrderById))
        {
            query = query.With<ContentItemIndex>().OrderByDescending(index => index.DisplayText);
        }

        return Task.FromResult(query);
    }
}
