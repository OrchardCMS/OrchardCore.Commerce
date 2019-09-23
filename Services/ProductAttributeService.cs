using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Services
{
    public class ProductAttributeService : IProductAttributeService
    {
        private readonly IList<IProductAttributeProvider> _attributeProviders;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOptions<ContentOptions> _contentOptions;
        private readonly IMemoryCache _cache;

        public ProductAttributeService(
            IList<IProductAttributeProvider> attributeProviders,
            IContentDefinitionManager contentDefinitionManager,
            IOptions<ContentOptions> contentOptions,
            IMemoryCache cache)
        {
            _attributeProviders = attributeProviders;
            _contentDefinitionManager = contentDefinitionManager;
            _contentOptions = contentOptions;
            _cache = cache;
        }

        public IProductAttributeValue Parse(ContentPartFieldDefinition attributeFieldDefinition, string value)
            => _attributeProviders.Select(p => p.Parse(attributeFieldDefinition, value)).First(v => v != null);

        public IEnumerable<FieldDescription> GetProductAttributeFields(ContentItem product)
        {
            var productAttributeTypes = GetProductAttributeFieldTypes();
            return _contentDefinitionManager
                .GetTypeDefinition(product.ContentType)
                .Parts
                .SelectMany(part => part.PartDefinition.Fields
                    .Where(field => productAttributeTypes.ContainsKey(field.FieldDefinition.Name))
                    .Select(field => new FieldDescription(
                        name: field.Name,
                        partName: part.Name,
                        field: product.Get<ContentPart>(part.Name)?.Get(productAttributeTypes[field.FieldDefinition.Name], field.Name) as ContentField)));
        }

        private IDictionary<string, Type> GetProductAttributeFieldTypes()
            => _contentOptions
            .Value.ContentFieldOptions
            .Select(f => f.Type)
            .Where(t => typeof(ProductAttributeField).IsAssignableFrom(t))
            .ToDictionary(t => t.Name);
    }
}
