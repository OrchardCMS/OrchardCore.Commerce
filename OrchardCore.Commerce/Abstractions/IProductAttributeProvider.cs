using System.Text.Json;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductAttributeProvider
    {
        IProductAttributeValue Parse(
            ContentTypePartDefinition partDefinition,
            ContentPartFieldDefinition attributeFieldDefinition,
            string[] value);

        public IProductAttributeValue Parse(
            ContentTypePartDefinition partDefinition,
            ContentPartFieldDefinition attributeFieldDefinition,
            string value)
            => Parse(partDefinition, attributeFieldDefinition, new[] { value });

        IProductAttributeValue CreateFromJsonElement(
            ContentTypePartDefinition partDefinition,
            ContentPartFieldDefinition attributeFieldDefinition,
            JsonElement value);
    }
}
