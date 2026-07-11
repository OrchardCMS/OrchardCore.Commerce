using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Inventory;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
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
        var productAttributeTypes = GetProductAttributeFieldTypes(product);

        ProductAttributeField GetContentField(
            ContentTypePartDefinition typePartDefinition,
            ContentPartFieldDefinition partFieldDefinition) =>
            product
                .Get<ContentPart>(typePartDefinition.Name)
                ?.Get(
                    productAttributeTypes[partFieldDefinition.FieldDefinition.Name],
                    partFieldDefinition.Name) as ProductAttributeField;

        return (await _contentDefinitionManager.GetTypeDefinitionAsync(product.ContentType))
            .Parts
            .SelectMany(typePartDefinition => typePartDefinition.PartDefinition.Fields
                .Where(partFieldDefinition => productAttributeTypes.ContainsKey(partFieldDefinition.FieldDefinition.Name))
                .Select(partFieldDefinition =>
                {
                    var field = GetContentField(typePartDefinition, partFieldDefinition);
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
            var filteredInventory = inventoryPart.FilterOutdatedEntries();
            part.CanBeBought.Clear();

            // If an inventory's value is below 1 and back ordering is not allowed, corresponding
            // CanBeBought entry needs to be set to false; should be set to true otherwise.
            foreach (var inventory in filteredInventory)
            {
                part.CanBeBought[inventory.Key] = inventoryPart.AllowsBackOrder.Value || inventory.Value >= 1;
            }

            // If SKU was updated, CanBeBought keys also need to be updated.
            if (part.Sku != skuBefore)
            {
                UpdateAvailabilityKeys(part, filteredInventory.Count);
            }
        }
        else
        {
            part.CanBeBought[part.ContentItem.ContentItemId] = true;
        }
    }

    private static void UpdateAvailabilityKeys(ProductPart part, int inventoryCount)
    {
        var newAvailabilities = new Dictionary<string, bool>();
        foreach (var entry in part.CanBeBought)
        {
            var updatedKey = inventoryCount > 1
                ? $"{part.Sku}-{entry.Key.Split('-')[^1]}"
                : part.Sku;

            newAvailabilities.Add(updatedKey, entry.Value);
        }

        part.CanBeBought.Clear();
        part.CanBeBought.AddRange(newAvailabilities);
    }

    private ProductAttributeFieldSettings GetFieldSettings(
        ContentPartFieldDefinition partFieldDefinition,
        ProductAttributeField field) =>
        field
            ?.GetType()
            // Using that type parameter arbitrarily, any one of the concrete attribute settings types would have done.
            .GetMethod(
                nameof(ProductAttributeField<TextProductAttributeFieldSettings>.GetSettings),
                BindingFlags.Instance | BindingFlags.Public)
            ?.Invoke(field, [partFieldDefinition]) as ProductAttributeFieldSettings;

    private static Dictionary<string, Type> GetProductAttributeFieldTypes(ContentItem product) =>
        product.OfType<ContentPart>()
            .SelectMany(parts => parts.OfType<ProductAttributeField>())
            .Select(field => field.GetType())
            .Distinct()
            .ToDictionary(type => type.Name);
}
