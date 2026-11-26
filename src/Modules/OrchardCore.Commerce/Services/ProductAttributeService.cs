using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Extensions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Inventory;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class ProductAttributeService : IProductAttributeService
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ProductAttributeService(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public async Task<IEnumerable<ProductAttributeDescription>> GetProductAttributeFieldsAsync(ContentItem product)
    {
        var productAttributeTypes = product.GetFieldTypes<ProductAttributeField>();
        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(product.ContentType);

        return contentTypeDefinition
            .Parts
            .SelectMany(typePartDefinition => typePartDefinition.PartDefinition.Fields
                .Where(partFieldDefinition => productAttributeTypes.ContainsKey(partFieldDefinition.FieldDefinition.Name))
                .Select(partFieldDefinition =>
                {
                    var field = GetContentField(product, productAttributeTypes, typePartDefinition, partFieldDefinition);
                    var settings = GetFieldSettings(partFieldDefinition, field);
                    return new ProductAttributeDescription(
                        name: partFieldDefinition.Name,
                        partName: typePartDefinition.Name,
                        field: field,
                        settings: settings);
                }))
            .Where(description => description.Field != null);
    }

    public (ContentTypePartDefinition PartDefinition, ContentPartFieldDefinition FieldDefinition)
        GetFieldDefinition(ContentTypeDefinition type, string attributeName)
    {
        var partAndField = attributeName.Split('.');
        var partName = partAndField[0];
        var fieldName = partAndField[1];

        return type
            .Parts
            .Where(partDefinition => partDefinition.Name == partName)
            .SelectMany(partDefinition => partDefinition
                .PartDefinition
                .Fields
                .Select(fieldDefinition => (PartDefinition: partDefinition, FieldDefinition: fieldDefinition))
                .Where(pair => pair.FieldDefinition.Name == fieldName))
            .FirstOrDefault();
    }

    public void UpdateCanBeBoughtForProductAttributeField(ProductPart part, string skuBefore)
    {
        if (part.ContentItem.TryGet<InventoryPart>(out var inventoryPart))
        {
            // If an inventory's value is below 1 and back ordering is not allowed, corresponding
            // CanBeBought entry needs to be set to false; should be set to true otherwise.
            var filteredInventory = inventoryPart
                .FilterOutdatedEntries()
                .ToDictionary(pair => pair.Key, pair => inventoryPart.AllowsBackOrder.Value || pair.Value >= 1);
            part.CanBeBought.SetItems(filteredInventory);

            // If SKU was updated, CanBeBought keys also need to be updated.
            if (part.Sku != skuBefore)
            {
                UpdateAvailabilityKeys(part, filteredInventory.Count, skuBefore);
            }
        }
        else
        {
            part.CanBeBought[part.ContentItem.ContentItemId] = true;
        }
    }

    public async Task<IEnumerable<ProductAttributeDescription>> GetProductAttributesRestrictedToPredefinedValuesAsync(ContentItem product) =>
        (await GetProductAttributeFieldsAsync(product))
        .Where(description => description.Settings is
            IPredefinedValuesProductAttributeFieldSettings { RestrictToPredefinedValues: true })
        .OrderBy(description => description.PartName, StringComparer.Ordinal)
        .ThenBy(description => description.Name, StringComparer.Ordinal);

    public async Task<string> GetProductKeyAsync(
        ContentItem product,
        ICollection<IProductAttributeValue> attributes,
        IShoppingCartSerializer serializer)
    {
        var productPart = product.GetOrCreate<ProductPart>();
        var predefinedAttributeValues = (await GetProductAttributesRestrictedToPredefinedValuesAsync(product))
            .Select(attr => attr.PartName + "." + attr.Name)
            .ToHashSet();

        if (predefinedAttributeValues.Count == 0) return null;

        if (attributes.HasRawAttributes())
        {
            attributes.SetItems(await serializer.PostProcessAttributesAsync(attributes, productPart));
        }

        var predefinedAttributes = attributes
            .OfType<IPredefinedValuesProductAttributeValue>()
            .Where(attribute => predefinedAttributeValues.Contains(attribute.AttributeName))
            .OrderBy(value => value.AttributeName, StringComparer.Ordinal);

        return string
            .Join('-', predefinedAttributes.Select(attr => attr.UntypedPredefinedValue).Where(value => value != null))
            .HtmlClassify();
    }

    public async Task UpdateInventoryPartFromProductAttributesAsync(ContentItem product)
    {
        if (!product.Has<PriceVariantsPart>() || !product.Has<InventoryPart>()) return;

        var variantKeys = (await this.GetProductAttributesCombinationsAsync(product))
            .Select(key => $"{product.GetOrCreate<ProductPart>().Sku}-{key}".ToUpperInvariant())
            .ToList();

        product.Alter<InventoryPart>(part =>
        {
            part.Inventory.SetItems(variantKeys.Select(key => 
                new KeyValuePair<string, int>(key, part.Inventory.TryGetValue(key, out var value) ? value : 0)));
            part.ResetInventoryKeys();
        });
    }

    private static void UpdateAvailabilityKeys(ProductPart part, int inventoryCount, string skuBefore)
    {
        var newAvailabilities = new Dictionary<string, bool>();
        foreach (var entry in part.CanBeBought)
        {
            var updatedKey = inventoryCount > 1
                ? InventoryPart.GetUpdatedInventoryKey(entry.Key, skuBefore, part.Sku)
                : part.Sku;

            newAvailabilities.Add(updatedKey, entry.Value);
        }

        part.CanBeBought.SetItems(newAvailabilities);
    }

    private static ProductAttributeField GetContentField(
        ContentItem product,
        IDictionary<string, Type> productAttributeTypes,
        ContentTypePartDefinition typePartDefinition,
        ContentPartFieldDefinition partFieldDefinition) =>
        product
                .Get<ContentPart>(typePartDefinition.Name)?
                .Get(productAttributeTypes[partFieldDefinition.FieldDefinition.Name], partFieldDefinition.Name)
            as ProductAttributeField;

    private ProductAttributeFieldSettings GetFieldSettings(
        ContentPartFieldDefinition partFieldDefinition,
        ProductAttributeField field) =>
        field
            ?.GetType()
            .GetMethod(
                nameof(ProductAttributeField<>.GetSettings),
                BindingFlags.Instance | BindingFlags.Public)
            ?.Invoke(field, [partFieldDefinition]) as ProductAttributeFieldSettings;
}
