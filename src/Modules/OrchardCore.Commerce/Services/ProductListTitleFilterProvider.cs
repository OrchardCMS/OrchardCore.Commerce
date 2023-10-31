using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public class ProductListTitleFilterProvider : IProductListFilterProvider
{
    public const string TitleFilterId = "title";
    public const string TitleAscOrderById = "titleAsc";
    public const string TitleDescOrderById = "titleDesc";

    public int Order => 10;

    public Task<bool> IsApplicableAsync(ProductListPart productList) => Task.FromResult(true);

    public Task<IEnumerable<string>> GetOrderByOptionIdsAsync(ProductListPart productList) =>
        Task.FromResult<IEnumerable<string>>(new[] { TitleAscOrderById, TitleDescOrderById });

    public Task<IEnumerable<string>> GetFilterIdsAsync(ProductListPart productListPart) =>
        Task.FromResult<IEnumerable<string>>(new[] { TitleFilterId });

    public Task<IQuery<ContentItem>> BuildQueryAsync(ProductListFilterContext context)
    {
        var query = context.Query;
        if (context.FilterParameters.FilterValues.TryGetValue(TitleFilterId, out var title))
        {
            query = query.With<ContentItemIndex>(index => index.DisplayText.Contains(title));
        }

        if (context.FilterParameters.OrderBy.EqualsOrdinalIgnoreCase(TitleAscOrderById))
        {
            query = query.With<ContentItemIndex>().OrderBy(index => index.DisplayText);
        }
        else if (context.FilterParameters.OrderBy.EqualsOrdinalIgnoreCase(TitleDescOrderById))
        {
            query = query.With<ContentItemIndex>().OrderByDescending(index => index.DisplayText);
        }

        return Task.FromResult(query);
    }
}
