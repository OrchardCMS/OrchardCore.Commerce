using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using System;
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
    private readonly IPredefinedValuesProductAttributeService _predefinedValuesService;
    private readonly Lazy<IShoppingCartSerializer> _shoppingCartSerializer;

    public ProductService(
        ISession session,
        IContentManager contentManager,
        IContentDefinitionManager contentDefinitionManager,
        IPredefinedValuesProductAttributeService predefinedValuesService,
        Lazy<IShoppingCartSerializer> shoppingCartSerializer)
    {
        _session = session;
        _contentManager = contentManager;
        _contentDefinitionManager = contentDefinitionManager;
        _predefinedValuesService = predefinedValuesService;
        _shoppingCartSerializer = shoppingCartSerializer;
    }

    public virtual async Task<IEnumerable<ProductPart>> GetProductsAsync(IEnumerable<string> skus)
    {
        var trimmedSkus = skus.Select(sku => sku.Split('-')[0]);

        var utcNow = DateTime.UtcNow;
        var contentItemIds = (await _session
                .QueryIndex<ProductPartIndex>(index =>
                    index.Sku.IsIn(trimmedSkus) &&
                    (index.StartTimeUtc == null || index.StartTimeUtc <= utcNow) &&
                    (index.EndTimeUtc == null || index.EndTimeUtc >= utcNow))
                .ListAsync())
            .Select(idx => idx.ContentItemId)
            .Distinct();

        var contentItems = await _contentManager.GetAsync(contentItemIds);

        // We have to replicate some things that BuildDisplayAsync does to fill part.Elements with the fields. We can't
        // use BuildDisplayAsync directly because it requires a BuildDisplayContext.
        return await FillContentItemsAndGetProductPartsAsync(contentItems);
    }

    public async Task<string> GetOrderFullSkuAsync(ShoppingCartItem item, ProductPart productPart)
    {
        if (productPart == null) return item.ProductSku;

        var attributesRestrictedToPredefinedValues = (await _predefinedValuesService
            .GetProductAttributesRestrictedToPredefinedValuesAsync(productPart.ContentItem))
            .Select(attr => attr.PartName + "." + attr.Name)
            .ToHashSet();

        if (attributesRestrictedToPredefinedValues.Count == 0) return item.ProductSku;

        if (item.HasRawAttributes())
        {
            item.Attributes.SetItems(await _shoppingCartSerializer.Value.PostProcessAttributesAsync(item.Attributes, productPart));
        }

        var variantKey = item.GetVariantKeyFromAttributes(attributesRestrictedToPredefinedValues);
        var fullSku = item.Attributes.Any()
            ? item.ProductSku + "-" + variantKey
            : string.Empty;

        return fullSku.ToUpperInvariant();
    }

    public async Task<IEnumerable<ProductPart>> GetProductsByContentItemVersionsAsync(IEnumerable<string> contentItemVersions)
    {
        // There is no GetVersionAsync that accepts a collection.
        var contentItems = await contentItemVersions.AwaitEachAsync(_contentManager.GetVersionAsync);

        // We have to replicate some things that BuildDisplayAsync does to fill part.Elements with the fields. We can't
        // use BuildDisplayAsync directly because it requires a BuildDisplayContext.
        return await FillContentItemsAndGetProductPartsAsync(contentItems);
    }

    public string GetVariantKey(string sku) =>
        sku.Partition("-").Right ??
        throw new ArgumentException("The SKU doesn't contain a dash. Is it a product variant SKU?", nameof(sku));

    public async Task<(PriceVariantsPart Part, string VariantKey)> GetExactVariantAsync(string sku)
    {
        var productPart = await this.GetProductAsync(sku);
        var priceVariantsPart = productPart?.ContentItem.As<PriceVariantsPart>();

        return (priceVariantsPart, GetVariantKey(sku));
    }

    private static void FillField(ContentPart part, ContentPartFieldDefinition field)
    {
        // We can only get the type of field in a string, so we need to convert that to an actual type.
        var typeOfField = Type.GetType("OrchardCore.Commerce.Fields." + field.FieldDefinition.Name);

        if (typeOfField != null)
        {
            var fieldName = field.Name;

            // We won't do anything with the result because we don't need to, but this is what fills the
            // fields in the original code.
            part.Get(typeOfField, fieldName);
        }
    }

    private async Task<List<ProductPart>> FillContentItemsAndGetProductPartsAsync(IEnumerable<ContentItem> contentItems)
    {
        var results = new List<ProductPart>();

        foreach (var contentItem in contentItems)
        {
            var contentItemsPartDefinitions = (await _contentDefinitionManager
                .GetTypeDefinitionAsync(contentItem.ContentType))
                .Parts;

            foreach (var partDefinition in contentItemsPartDefinitions)
            {
                var contentFields = partDefinition.PartDefinition.Fields;
                var part = contentItem.Get<ContentPart>(partDefinition.Name);

                foreach (var field in contentFields)
                {
                    FillField(part, field);
                }
            }

            results.Add(contentItem.As<ProductPart>());
        }

        return results;
    }
}
