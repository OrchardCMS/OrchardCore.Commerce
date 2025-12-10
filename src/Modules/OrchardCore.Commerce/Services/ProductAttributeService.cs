using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
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
