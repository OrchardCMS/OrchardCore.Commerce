using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Commerce.Fields;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Tests
{
    public class FakeContentDefinitionManager : IContentDefinitionManager
    {
        public IChangeToken ChangeToken => throw new System.NotImplementedException();
        public void DeletePartDefinition(string name) => throw new System.NotImplementedException();
        public void DeleteTypeDefinition(string name) => throw new System.NotImplementedException();
        public ContentPartDefinition GetPartDefinition(string name) => throw new System.NotImplementedException();

        public ContentTypeDefinition GetTypeDefinition(string name)
            => new ContentTypeDefinition(name, name, new[] {
                    new ContentTypePartDefinition("product", new ContentPartDefinition("product", new[] {
                        new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(BooleanProductAttributeField)), "attr1", null),
                        new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(TextProductAttributeField)), "attr2", null),
                        new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(NumericProductAttributeField)), "attr3", null)
                    }, null), null)
            }, null);

        public Task<int> GetTypesHashAsync() => throw new System.NotImplementedException();
        public IEnumerable<ContentPartDefinition> ListPartDefinitions() => throw new System.NotImplementedException();
        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions() => throw new System.NotImplementedException();
        public void StorePartDefinition(ContentPartDefinition contentPartDefinition) => throw new System.NotImplementedException();
        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition) => throw new System.NotImplementedException();
    }
}
