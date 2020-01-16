using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
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
        private readonly IOptions<ContentOptions> _contentOptions;
        private readonly IMemoryCache _cache;

        public ProductAttributeService(
            IEnumerable<IProductAttributeProvider> attributeProviders,
            IContentDefinitionManager contentDefinitionManager,
            IOptions<ContentOptions> contentOptions,
            IMemoryCache cache)
        {
            _attributeProviders = attributeProviders;
            _contentDefinitionManager = contentDefinitionManager;
            _contentOptions = contentOptions;
            _cache = cache;
        }

        public IEnumerable<ProductAttributeDescription> GetProductAttributeFields(ContentItem product)
        {
            var productAttributeTypes = GetProductAttributeFieldTypes();

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

        public IEnumerable<IEnumerable<string>> GetProductAttributesPredefinedValues(ContentItem product)
        {
            return GetProductAttributeFields(product).Select(x =>
                {
                    if (x.Settings is TextProductAttributeFieldSettings textSettings && textSettings.RestrictToPredefinedValues)
                    {
                        return textSettings.PredefinedValues.ToList();
                    }
                    return null;
                })
                .Where(x => x != null)
                .ToList();
        }

        public IEnumerable<string> GetProductAttributesCombinations(ContentItem product)
        {
            return CartesianProduct(GetProductAttributesPredefinedValues(product))
                .Select(x => string.Join("-", x));
        }

        private IDictionary<string, Type> GetProductAttributeFieldTypes()
            => _contentOptions
            .Value.ContentFieldOptions
            .Select(f => f.Type)
            .Where(t => typeof(ProductAttributeField).IsAssignableFrom(t))
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

        private IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item })
                );
        }
    }
}