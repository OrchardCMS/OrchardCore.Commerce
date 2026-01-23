using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Commerce.Services;

public class ProductListService : IProductListService
{
    private readonly ISession _session;
    private readonly IEnumerable<IProductListFilterProvider> _productListQueryProviders;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ProductListService(
        ISession session,
        IEnumerable<IProductListFilterProvider> productListQueryProviders,
        IContentDefinitionManager contentDefinitionManager)
    {
        _session = session;
        _productListQueryProviders = productListQueryProviders;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<ProductList> GetProductsAsync(ProductListPart productList, ProductListFilterParameters filterParameters)
    {
        ArgumentNullException.ThrowIfNull(productList);
        ArgumentNullException.ThrowIfNull(filterParameters);

        var productTypes = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Where(type => type.Parts.Any(part => part.PartDefinition.Name == nameof(ProductPart)))
            .Select(type => type.Name)
            .ToArray();

        var applicableProviders = await GetOrderedApplicableProvidersAsync(productList);

        var query = _session.Query<ContentItem>();
        query = query.With<ContentItemIndex>(index => index.ContentType.IsIn(productTypes) && index.Published);
        
        // Filter products by start and end time
        var utcNow = DateTime.UtcNow;
        query = query.With<ProductPartIndex>(index =>
            (index.StartTimeUtc == null || index.StartTimeUtc <= utcNow) &&
            (index.EndTimeUtc == null || index.EndTimeUtc >= utcNow));

        var context = new ProductListFilterContext
        {
            ProductList = productList,
            FilterParameters = filterParameters,
            Query = query,
        };

        if (string.IsNullOrEmpty(filterParameters.OrderBy))
        {
            filterParameters.OrderBy = ProductListTitleFilterProvider.TitleAscOrderById;
        }

        foreach (var provider in applicableProviders)
        {
            context.Query = await provider.BuildQueryAsync(context) ?? context.Query;
        }

        var totalItemCount = await query.CountAsync();
        var contentItems = await query.PaginateAsync(filterParameters.Pager.Page - 1, filterParameters.Pager.PageSize);

        return new ProductList
        {
            Products = contentItems,
            TotalItemCount = totalItemCount,
        };
    }

    public async Task<IEnumerable<string>> GetOrderByOptionsAsync(ProductListPart productList)
    {
        ArgumentNullException.ThrowIfNull(productList);

        var applicableProviders = await GetOrderedApplicableProvidersAsync(productList);

        return (await applicableProviders.AwaitEachAsync(provider => provider.GetOrderByOptionIdsAsync(productList)))
            .SelectMany(options => options);
    }

    public async Task<IEnumerable<string>> GetFilterIdsAsync(ProductListPart productList)
    {
        ArgumentNullException.ThrowIfNull(productList);

        var applicableProviders = await GetOrderedApplicableProvidersAsync(productList);

        return (await applicableProviders.AwaitEachAsync(provider => provider.GetFilterIdsAsync(productList)))
            .SelectMany(options => options);
    }

    private async Task<IList<IProductListFilterProvider>> GetOrderedApplicableProvidersAsync(ProductListPart productList) =>
        [.. (await _productListQueryProviders
                .WhereAsync(async provider => await provider.IsApplicableAsync(productList)))
            .OrderBy(provider => provider.Order)];
}
