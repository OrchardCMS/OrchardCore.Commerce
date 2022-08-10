using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
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

    public ProductService(
        ISession session,
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager)
    {
        _session = session;
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<ProductPart>> GetProductsAsync(IEnumerable<string> skus)
    {
        var contentItemIds = (await _session
                .QueryIndex<ProductPartIndex>(index => index.Sku.IsIn(skus))
                .ListAsync())
            .Select(idx => idx.ContentItemId)
            .Distinct();

        var contentItems = await _contentManager.GetAsync(contentItemIds);

        // We need to replicate some things that BuildDisplayAsync does to fill part.Elements with the fields. We can't
        // use BuildDisplayAsync directly because it requires a BuildDisplayContext.
        foreach (var contentItem in contentItems)
        {
            var contentItemsPartName = contentItem.ContentType;

            var contentItemsPartFields = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType).Parts
                .FirstOrDefault(contentPartDefinition =>
                    contentPartDefinition.PartDefinition.Name == contentItemsPartName)
                .PartDefinition
                .Fields;

            var part = contentItem.Get<ContentPart>(contentItemsPartName);

            foreach (var field in contentItemsPartFields)
            {
                var fieldName = field.Name;

                switch (field.FieldDefinition.Name)
                {
                    case nameof(BooleanProductAttributeField):
                        part.Get<BooleanProductAttributeField>(fieldName);
                        break;

                    case nameof(NumericProductAttributeField):
                        part.Get<NumericProductAttributeField>(fieldName);
                        break;

                    case nameof(TextProductAttributeField):
                        part.Get<TextProductAttributeField>(fieldName);
                        break;

                    default:
                        continue;
                }
            }
        }

        return contentItems.Select(item => item.As<ProductPart>());
    }
}
