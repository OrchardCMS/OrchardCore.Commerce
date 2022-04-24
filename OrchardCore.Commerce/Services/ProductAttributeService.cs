using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Services;

public class ProductAttributeService : IProductAttributeService
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ProductAttributeService(IContentDefinitionManager contentDefinitionManager) =>
        _contentDefinitionManager = contentDefinitionManager;

    public IEnumerable<ProductAttributeDescription> GetProductAttributeFields(ContentItem product)
    {
        var productAttributeTypes = GetProductAttributeFieldTypes(product);

        ProductAttributeField GetContentField(ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition)
            => product.Get<ContentPart>(typePartDefinition.Name)
                ?.Get(productAttributeTypes[partFieldDefinition.FieldDefinition.Name], partFieldDefinition.Name) as ProductAttributeField;

        return _contentDefinitionManager
            .GetTypeDefinition(product.ContentType)
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

    private static IDictionary<string, Type> GetProductAttributeFieldTypes(ContentItem product)
        => product.OfType<ContentPart>()
            .SelectMany(parts => parts.OfType<ProductAttributeField>())
            .Select(t => t.GetType())
            .Distinct()
            .ToDictionary(t => t.Name);

    private ProductAttributeFieldSettings GetFieldSettings(ContentPartFieldDefinition partFieldDefinition, ProductAttributeField field) =>
        field
            ?.GetType()
            // Using that type parameter arbitrarily, any one of the concrete attribute settings types would have done.
            .GetMethod(
                nameof(ProductAttributeField<TextProductAttributeFieldSettings>.GetSettings),
                BindingFlags.Instance | BindingFlags.Public)
            ?.Invoke(field, new object[] { partFieldDefinition }) as ProductAttributeFieldSettings;
}
