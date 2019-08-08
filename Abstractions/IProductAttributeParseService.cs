using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductAttributeParseService
    {
        IProductAttributeValue Parse(ContentPartFieldDefinition attributeFieldDefinition, string value);
    }
}
