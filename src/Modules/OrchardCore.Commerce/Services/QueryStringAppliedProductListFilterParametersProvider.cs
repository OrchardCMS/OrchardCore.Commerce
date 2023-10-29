using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class QueryStringAppliedProductListFilterParametersProvider : IAppliedProductListFilterParametersProvider
{
    public const string QueryStringPrefix = "products.";

    private readonly IHttpContextAccessor _hca;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly ISiteService _siteService;

    public int Priority { get; } = 10;

    public QueryStringAppliedProductListFilterParametersProvider(
        IHttpContextAccessor hca,
        IUpdateModelAccessor updateModelAccessor,
        ISiteService siteService)
    {
        _hca = hca;
        _updateModelAccessor = updateModelAccessor;
        _siteService = siteService;
    }

    public async Task<ProductListFilterParameters> GetFilterParametersAsync(ProductListPart productList)
    {
        var queryStrings = _hca.HttpContext.Request.Query;
        var orderByValues = queryStrings
            .Where(queryString => queryString.Key.StartsWith(QueryStringPrefix + "orderBy", StringComparison.InvariantCulture))
            .SelectMany(queryString => queryString.Value)
            .ToList();
        var filterValues = queryStrings
            .Where(queryString => queryString.Key.StartsWith(QueryStringPrefix, StringComparison.InvariantCulture))
            .ToDictionary(
                queryString => queryString.Key[QueryStringPrefix.Length..],
                queryString => queryString.Value.FirstOrDefault());

        var pagerParameters = new PagerParameters();
        await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(pagerParameters);

        var filterParameters = new ProductListFilterParameters
        {
            Pager = new Pager(pagerParameters, (await _siteService.GetSiteSettingsAsync()).PageSize),
        };
        filterParameters.OrderBy.AddRange(orderByValues);
        filterParameters.FilterValues.AddRange(filterValues);

        return filterParameters;
    }
}
