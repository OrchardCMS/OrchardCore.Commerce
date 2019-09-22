using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductAttributeService
    {
        IProductAttributeValue Parse(ContentPartFieldDefinition attributeFieldDefinition, string value);
    }
}
