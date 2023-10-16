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
    private readonly IEnumerable<IProductListItemProvider> _productListQueryProviders;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ProductListService(
        ISession session,
        IEnumerable<IProductListItemProvider> productListQueryProviders,
        IContentDefinitionManager contentDefinitionManager)
    {
        _session = session;
        _productListQueryProviders = productListQueryProviders;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<ContentItem>> GetProductsAsync(ProductListPart productList, Pager pager)
    {
        if (productList is null)
        {
            throw new ArgumentNullException(nameof(productList));
        }

        var productTypes = _contentDefinitionManager.ListTypeDefinitions()
            .Where(type => type.Parts.Any(part => part.PartDefinition.Name == nameof(ProductPart)))
            .Select(type => type.Name)
            .ToArray();

        var applicableProviders = (await _productListQueryProviders
            .WhereAsync(async provider => await provider.CanHandleAsync(productList)))
            .OrderBy(provider => provider.Order)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ToArray();

        var query = _session.Query<ContentItem>();
        query = query.With<ContentItemIndex>(index => index.ContentType.IsIn(productTypes) && index.Published);

        var context = new ProductListQueryContext
        {
            ProductList = productList,
            Pager = pager,
            Query = query,
        };
        foreach (var provider in applicableProviders)
        {
            context.Query = await provider.BuildQueryAsync(context) ?? context.Query;
        }

        var contentItems = await query.ListAsync();
        foreach (var provider in applicableProviders)
        {
            context.QueriedProducts = await provider.PostProcessListAsync(context) ?? context.QueriedProducts;
        }

        return contentItems;
    }
}
