using Microsoft.Extensions.Primitives;
using OrchardCore.Commerce.Fields;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

internal sealed class FakeContentDefinitionManager : IContentDefinitionManager
{
    public IChangeToken ChangeToken => throw new NotSupportedException();
    public Task DeletePartDefinitionAsync(string name) => throw new NotSupportedException();
    public Task DeleteTypeDefinitionAsync(string name) => throw new NotSupportedException();

    public Task<string> GetIdentifierAsync() => throw new NotSupportedException();

    public Task<ContentPartDefinition> GetPartDefinitionAsync(string name) => throw new NotSupportedException();

    public Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name) =>
         Task.FromResult(new ContentTypeDefinition(
            name,
            name,
            [
                new ContentTypePartDefinition(
                    "ProductPart1",
                    new ContentPartDefinition(
                        "ProductPartType",
                        [
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(BooleanProductAttributeField)),
                                "foobool",
                                []),
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(BooleanField)),
                                "barbool",
                                []),
                        ],
                        []),
                    []),
                new ContentTypePartDefinition(
                    "ProductPart2",
                    new ContentPartDefinition(
                        "ProductPartType2",
                        [
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(TextProductAttributeField)),
                                "footext",
                                []),
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(TextField)),
                                "bartext",
                                []),
                        ],
                        []),
                    []),
                new ContentTypePartDefinition(
                    "ProductPart3",
                    new ContentPartDefinition(
                        "product",
                        [
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(BooleanProductAttributeField)),
                                "attr1",
                                []),
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(TextProductAttributeField)),
                                "attr2",
                                []),
                            new ContentPartFieldDefinition(
                                new ContentFieldDefinition(nameof(NumericProductAttributeField)),
                                "attr3",
                                []),
                        ],
                        []),
                    []),
            ],
            []));

    public Task<int> GetTypesHashAsync() => throw new NotSupportedException();
    public Task<IEnumerable<ContentPartDefinition>> ListPartDefinitionsAsync() => throw new NotSupportedException();
    public Task<IEnumerable<ContentTypeDefinition>> ListTypeDefinitionsAsync() => throw new NotSupportedException();

    public Task<ContentPartDefinition> LoadPartDefinitionAsync(string name) => throw new NotSupportedException();

    public Task<IEnumerable<ContentPartDefinition>> LoadPartDefinitionsAsync() => throw new NotSupportedException();

    public Task<ContentTypeDefinition> LoadTypeDefinitionAsync(string name) => throw new NotSupportedException();

    public Task<IEnumerable<ContentTypeDefinition>> LoadTypeDefinitionsAsync() => throw new NotSupportedException();

    public Task StorePartDefinitionAsync(ContentPartDefinition contentPartDefinition) => throw new NotSupportedException();
    public Task StoreTypeDefinitionAsync(ContentTypeDefinition contentTypeDefinition) => throw new NotSupportedException();
}
