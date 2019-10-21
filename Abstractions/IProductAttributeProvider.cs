using System.Text.Json;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductAttributeProvider
    {
        IProductAttributeValue Parse(ContentPartFieldDefinition attributeFieldDefinition, string[] value);
        IProductAttributeValue CreateFromJsonElement(ContentPartFieldDefinition attributeFieldDefinition, JsonElement value);
    }
}
