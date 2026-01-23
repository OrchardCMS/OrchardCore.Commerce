using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tests.Fakes;

public class FakeContentManager : NullContentManager
{
    public override Task<TAspect> PopulateAspectAsync<TAspect>(IContent content, TAspect aspect)
    {
        if (typeof(TAspect) != typeof(ContentItemMetadata)) throw new NotSupportedException();

        var metadata = new ContentItemMetadata { DisplayRouteValues = [] };
        return Task.FromResult((TAspect)(object)metadata);
    }
}

public class NullContentManager : IContentManager
{
    public virtual Task<ContentItem> NewAsync(string contentType) => Task.FromResult(new ContentItem());

    public virtual Task UpdateAsync(ContentItem contentItem) => Task.CompletedTask;

    public virtual Task<bool> CreateAsync(ContentItem contentItem, VersionOptions options = null) =>
        Task.FromResult(false);

    public virtual Task<ContentValidateResult> CreateContentItemVersionAsync(ContentItem contentItem) =>
        Task.FromResult(new ContentValidateResult());

    public virtual Task<ContentValidateResult> UpdateContentItemVersionAsync(
        ContentItem updatingVersion,
        ContentItem updatedVersion) =>
        Task.FromResult(new ContentValidateResult());

    public virtual Task ImportAsync(IEnumerable<ContentItem> contentItems) => Task.CompletedTask;

    public virtual Task<ContentValidateResult> ValidateAsync(ContentItem contentItem) =>
        Task.FromResult(new ContentValidateResult());

    public virtual Task<ContentValidateResult> RestoreAsync(ContentItem contentItem) =>
        Task.FromResult(new ContentValidateResult());

    public virtual Task<ContentItem> GetAsync(string contentItemId, VersionOptions options = null) =>
        Task.FromResult(new ContentItem());

    public virtual Task<IEnumerable<ContentItem>> GetAsync(
        IEnumerable<string> contentItemIds,
        VersionOptions options = null) =>
        Task.FromResult<IEnumerable<ContentItem>>([]);

    public virtual Task<ContentItem> GetVersionAsync(string contentItemVersionId) =>
        Task.FromResult(new ContentItem());

    public virtual Task<IEnumerable<ContentItem>> GetAllVersionsAsync(string contentItemId) =>
        Task.FromResult<IEnumerable<ContentItem>>([]);

    public virtual Task<ContentItem> LoadAsync(ContentItem contentItem) =>
        Task.FromResult(new ContentItem());

    public virtual Task RemoveAsync(ContentItem contentItem) => Task.CompletedTask;

    public virtual Task DiscardDraftAsync(ContentItem contentItem) => Task.CompletedTask;

    public virtual Task SaveDraftAsync(ContentItem contentItem) => Task.CompletedTask;

    public virtual Task<bool> PublishAsync(ContentItem contentItem) => Task.FromResult(false);

    public virtual Task<bool> UnpublishAsync(ContentItem contentItem) => Task.FromResult(false);

    public virtual Task<TAspect> PopulateAspectAsync<TAspect>(IContent content, TAspect aspect) =>
        Task.FromResult<TAspect>(default);

    public virtual Task<ContentItem> CloneAsync(ContentItem contentItem) =>
        Task.FromResult(new ContentItem());
}
