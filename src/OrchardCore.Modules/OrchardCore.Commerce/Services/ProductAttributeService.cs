using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Services
{
    public class ProductAttributeService : IProductAttributeService
    {
        private readonly IEnumerable<IProductAttributeProvider> _attributeProviders;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IMemoryCache _cache;

        public ProductAttributeService(
            IEnumerable<IProductAttributeProvider> attributeProviders,
            IContentDefinitionManager contentDefinitionManager,
            IMemoryCache cache)
        {
            _attributeProviders = attributeProviders;
            _contentDefinitionManager = contentDefinitionManager;
            _cache = cache;
        }

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

        private IDictionary<string, Type> GetProductAttributeFieldTypes(ContentItem product)
            => product.OfType<ContentPart>()
                .SelectMany(parts => parts.OfType<ProductAttributeField>())
                .Select(t => t.GetType())
                .ToDictionary(t => t.Name);

        private ProductAttributeFieldSettings GetFieldSettings(ContentPartFieldDefinition partFieldDefinition, ProductAttributeField field)
        {
            return field
                ?.GetType()
                ?.GetMethod(
                    // Using that type parameter arbitrarily, any one of the concrete attribute settings types would have done.
                    nameof(ProductAttributeField<TextProductAttributeFieldSettings>.GetSettings),
                    BindingFlags.Instance | BindingFlags.Public)
                ?.Invoke(field, new[] { partFieldDefinition }) as ProductAttributeFieldSettings;
        }
    }
}
