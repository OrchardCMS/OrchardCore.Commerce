using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductAttributeProvider
    {
        IProductAttributeValue Parse(ContentPartFieldDefinition attributeFieldDefinition, string value);
    }
}
