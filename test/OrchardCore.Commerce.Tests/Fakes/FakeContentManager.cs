using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakeContentManager : IContentManager
{
    public Task<ContentItem> CloneAsync(ContentItem contentItem) => throw new NotSupportedException();

    public Task CreateAsync(ContentItem contentItem, VersionOptions options, bool invokeUpdateCallbacks) =>
        throw new NotSupportedException();

    public Task CreateAsync(ContentItem contentItem, VersionOptions options) =>
        CreateAsync(contentItem, options, invokeUpdateCallbacks: false);

    public Task<ContentValidateResult> CreateContentItemVersionAsync(ContentItem contentItem) =>
        throw new NotSupportedException();

    public Task DiscardDraftAsync(ContentItem contentItem) => throw new NotSupportedException();

    public Task<ContentItem> GetAsync(string id) => throw new NotSupportedException();

    public Task<ContentItem> GetAsync(string id, VersionOptions options) => throw new NotSupportedException();

    public Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, bool latest = false) =>
        throw new NotSupportedException();

    public Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, VersionOptions options) =>
        throw new NotSupportedException();

    public Task<ContentItem> GetVersionAsync(string contentItemVersionId) => throw new NotSupportedException();

    public Task ImportAsync(IEnumerable<ContentItem> contentItems) => throw new NotSupportedException();

    public Task<ContentItem> LoadAsync(ContentItem contentItem) => throw new NotSupportedException();

    public Task<ContentItem> NewAsync(string contentType) => throw new NotSupportedException();

    public Task<TAspect> PopulateAspectAsync<TAspect>(IContent content, TAspect aspect)
    {
        if (typeof(TAspect) != typeof(ContentItemMetadata)) throw new NotSupportedException();
        var metadata = new ContentItemMetadata
        {
            DisplayRouteValues = [],
        };
        return Task.FromResult((TAspect)(object)metadata);
    }

    public Task PublishAsync(ContentItem contentItem) => throw new NotSupportedException();

    public Task RemoveAsync(ContentItem contentItem) => throw new NotSupportedException();

    public Task<ContentValidateResult> RestoreAsync(ContentItem contentItem) => throw new NotSupportedException();

    public Task SaveDraftAsync(ContentItem contentItem) => throw new NotSupportedException();

    public Task UnpublishAsync(ContentItem contentItem) => throw new NotSupportedException();

    public Task UpdateAsync(ContentItem contentItem) => throw new NotSupportedException();

    public Task<ContentValidateResult> UpdateContentItemVersionAsync(ContentItem updatingVersion, ContentItem updatedVersion) =>
        throw new NotSupportedException();

    public Task<ContentValidateResult> ValidateAsync(ContentItem contentItem) => throw new NotSupportedException();
}
