using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Navigation;
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
    private readonly IEnumerable<IProductFilterProvider> _productListQueryProviders;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ProductListService(
        ISession session,
        IEnumerable<IProductFilterProvider> productListQueryProviders,
        IContentDefinitionManager contentDefinitionManager)
    {
        _session = session;
        _productListQueryProviders = productListQueryProviders;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<ProductList> GetProductsAsync(ProductListPart productList, ProductListFilterParameters filterParameters)
    {
        if (productList is null)
        {
            throw new ArgumentNullException(nameof(productList));
        }

        var productTypes = _contentDefinitionManager.ListTypeDefinitions()
            .Where(type => type.Parts.Any(part => part.PartDefinition.Name == nameof(ProductPart)))
            .Select(type => type.Name)
            .ToArray();

        var applicableProviders = await GetOrderedApplicableProvidersAsync(productList);

        var query = _session.Query<ContentItem>();
        query = query.With<ContentItemIndex>(index => index.ContentType.IsIn(productTypes) && index.Published);

        var context = new ProductListFilterContext
        {
            ProductList = productList,
            FilterParameters = filterParameters,
            Query = query,
        };
        foreach (var provider in applicableProviders)
        {
            context.Query = await provider.BuildQueryAsync(context) ?? context.Query;
        }

        var totalItemCount = await query.CountAsync();

        var contentItems = await query
            .Skip(filterParameters.Pager.GetStartIndex())
            .Take(filterParameters.Pager.PageSize)
            .ListAsync();

        return new ProductList
        {
            Products = contentItems,
            TotalItemCount = totalItemCount,
        };
    }

    public async Task<IEnumerable<string>> GetOrderByOptionsAsync(ProductListPart productList)
    {
        if (productList is null)
        {
            throw new ArgumentNullException(nameof(productList));
        }

        var applicableProviders = await GetOrderedApplicableProvidersAsync(productList);

        var orderByOptions = new List<string>();
        foreach (var provider in applicableProviders)
        {
            orderByOptions.AddRange(await provider.GetOrderByOptionIdsAsync(productList));
        }

        return orderByOptions;
    }

    private async Task<IList<IProductFilterProvider>> GetOrderedApplicableProvidersAsync(ProductListPart productList) =>
        (await _productListQueryProviders
            .WhereAsync(async provider => await provider.CanHandleAsync(productList)))
        .OrderBy(provider => provider.Order)
        .ToList();
}
