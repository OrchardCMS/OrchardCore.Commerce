using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Commerce.Services;

public class ProductService : IProductService
{
    private readonly ISession _session;
    private readonly IContentManager _contentManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
    private readonly IContentDisplayHandler _contentDisplayHandler;

    public ProductService(
        ISession session,
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        ITypeActivatorFactory<ContentPart> contentPartTypeActivatorFactory,
        IContentDisplayHandler contentDisplayHandler)
    {
        _session = session;
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _contentPartFactory = contentPartTypeActivatorFactory;
        _contentDisplayHandler = contentDisplayHandler;
    }

    public async Task<IEnumerable<ProductPart>> GetProductsAsync(IEnumerable<string> skus)
    {
        var contentItemIds = (await _session
                .QueryIndex<ProductPartIndex>(index => index.Sku.IsIn(skus))
                .ListAsync())
            .Select(idx => idx.ContentItemId)
            .Distinct()
            .ToArray();

        var contentItems = await _contentManager.GetAsync(contentItemIds);

        // We need BuildDisplayAsync to fill part.Elements with the fields. We could extract the logic from
        // BuildDisplayAsync, but we would have to copy almost everything from there.
        if (contentItems.Any())
        {
            foreach (var contentItem in contentItems)
            {
                await _contentDisplayHandler.BuildDisplayAsync(contentItem, context: null);
            }
        }

        return contentItems.Select(item => item.As<ProductPart>());
    }
}
