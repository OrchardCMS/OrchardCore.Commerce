using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductAttributeService
    {
        IProductAttributeValue Parse(ContentPartFieldDefinition attributeFieldDefinition, string value);
        IEnumerable<ProductAttributeDescription> GetProductAttributeFields(ContentItem product);
    }
}
