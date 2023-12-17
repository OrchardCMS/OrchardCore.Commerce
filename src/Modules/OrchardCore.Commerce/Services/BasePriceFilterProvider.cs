using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

public class BasePriceFilterProvider : IProductListFilterProvider
{
    public const string PriceAscOrderById = "basePriceAsc";
    public const string PriceDescOrderById = "basePriceDesc";
    public int Order => 10;

    public Task<bool> IsApplicableAsync(ProductListPart productList) => Task.FromResult(true);

    public Task<IEnumerable<string>> GetOrderByOptionIdsAsync(ProductListPart productList) =>
        Task.FromResult<IEnumerable<string>>(new[] { PriceAscOrderById, PriceDescOrderById });

    public Task<IEnumerable<string>> GetFilterIdsAsync(ProductListPart productListPart) =>
        Task.FromResult(Enumerable.Empty<string>());

    public async Task<IQuery<ContentItem>> BuildQueryAsync(ProductListFilterContext context)
    {
        var query = context.Query;

        if (context.FilterParameters.OrderBy.EqualsOrdinalIgnoreCase(PriceAscOrderById))
        {
            query = query.With<PriceIndex>().OrderBy(index => index.MinPrice);
        }
        else if (context.FilterParameters.OrderBy.EqualsOrdinalIgnoreCase(PriceDescOrderById))
        {
            query = query.With<PriceIndex>().OrderByDescending(index => index.MaxPrice);
        }

        return query;
    }
}
