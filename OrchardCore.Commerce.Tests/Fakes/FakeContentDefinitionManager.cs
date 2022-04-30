using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Fields;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakeContentDefinitionManager : IContentDefinitionManager
{
    public IChangeToken ChangeToken => throw new NotSupportedException();
    public void DeletePartDefinition(string name) => throw new NotSupportedException();
    public void DeleteTypeDefinition(string name) => throw new NotSupportedException();

    public Task<string> GetIdentifierAsync() => throw new NotSupportedException();

    public ContentPartDefinition GetPartDefinition(string name) => throw new NotSupportedException();

    public ContentTypeDefinition GetTypeDefinition(string name) =>
        new(
            name,
            name,
            new[]
            {
                new ContentTypePartDefinition(
                    "ProductPart1",
                    new ContentPartDefinition(
                        "ProductPartType",
                        new[]
                        {
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(BooleanProductAttributeField)),
                                "foobool",
                                new JObject()),
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(BooleanField)),
                                "barbool",
                                new JObject()),
                        },
                        new JObject()),
                    new JObject()),
                new ContentTypePartDefinition(
                    "ProductPart2",
                    new ContentPartDefinition(
                        "ProductPartType2",
                        new[]
                        {
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(TextProductAttributeField)),
                                "footext",
                                new JObject()),
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(TextField)),
                                "bartext",
                                new JObject()),
                        },
                        new JObject()),
                    new JObject()),
                new ContentTypePartDefinition(
                    "ProductPart3",
                    new ContentPartDefinition(
                        "product",
                        new[]
                        {
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(BooleanProductAttributeField)),
                                "attr1",
                                new JObject()),
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(TextProductAttributeField)),
                                "attr2",
                                new JObject()),
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(NumericProductAttributeField)),
                                "attr3",
                                new JObject()),
                        },
                        new JObject()),
                    new JObject()),
            },
            new JObject());

    public Task<int> GetTypesHashAsync() => throw new NotSupportedException();
    public IEnumerable<ContentPartDefinition> ListPartDefinitions() => throw new NotSupportedException();
    public IEnumerable<ContentTypeDefinition> ListTypeDefinitions() => throw new NotSupportedException();

    public ContentPartDefinition LoadPartDefinition(string name) => throw new NotSupportedException();

    public IEnumerable<ContentPartDefinition> LoadPartDefinitions() => throw new NotSupportedException();

    public ContentTypeDefinition LoadTypeDefinition(string name) => throw new NotSupportedException();

    public IEnumerable<ContentTypeDefinition> LoadTypeDefinitions() => throw new NotSupportedException();

    public void StorePartDefinition(ContentPartDefinition contentPartDefinition) => throw new NotSupportedException();
    public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition) => throw new NotSupportedException();
}
