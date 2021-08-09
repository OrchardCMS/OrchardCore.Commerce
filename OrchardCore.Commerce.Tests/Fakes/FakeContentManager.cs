using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class FakeContentManager : IContentManager
    {
        public Task<ContentItem> CloneAsync(ContentItem contentItem) => throw new NotImplementedException();

        public Task CreateAsync(ContentItem contentItem, VersionOptions options, bool invokeUpdateCallbacks = false) => throw new System.NotImplementedException();

        public Task CreateAsync(ContentItem contentItem, VersionOptions options) => throw new NotImplementedException();

        public Task<ContentValidateResult> CreateContentItemVersionAsync(ContentItem contentItem) => throw new NotImplementedException();

        public Task DiscardDraftAsync(ContentItem contentItem) => throw new NotImplementedException();

        public Task<ContentItem> GetAsync(string id) => throw new NotImplementedException();

        public Task<ContentItem> GetAsync(string id, VersionOptions options) => throw new NotImplementedException();

        public Task<IEnumerable<ContentItem>> GetAsync(IEnumerable<string> contentItemIds, bool latest = false) => throw new NotImplementedException();

        public Task<ContentItem> GetVersionAsync(string contentItemVersionId) => throw new NotImplementedException();

        public Task ImportAsync(IEnumerable<ContentItem> contentItems) => throw new NotImplementedException();

        public Task<ContentItem> LoadAsync(ContentItem contentItem) => throw new NotImplementedException();

        public Task<ContentItem> NewAsync(string contentType) => throw new NotImplementedException();

        public async Task<TAspect> PopulateAspectAsync<TAspect>(IContent content, TAspect aspect)
        {
            if (typeof(TAspect) != typeof(ContentItemMetadata)) throw new NotImplementedException();
            var metadata = new ContentItemMetadata
            {
                DisplayRouteValues = new RouteValueDictionary()
            };
            return await Task.FromResult((TAspect)(object)metadata);
        }

        public Task PublishAsync(ContentItem contentItem) => throw new NotImplementedException();

        public Task RemoveAsync(ContentItem contentItem) => throw new NotImplementedException();

        public Task<ContentValidateResult> RestoreAsync(ContentItem contentItem) => throw new NotImplementedException();
        
        public Task SaveDraftAsync(ContentItem contentItem) => throw new NotImplementedException();

        public Task UnpublishAsync(ContentItem contentItem) => throw new NotImplementedException();

        public Task UpdateAsync(ContentItem contentItem) => throw new NotImplementedException();

        public Task<ContentValidateResult> UpdateContentItemVersionAsync(ContentItem updatingVersion, ContentItem updatedVersion) => throw new NotImplementedException();

        public Task<ContentValidateResult> ValidateAsync(ContentItem contentItem) => throw new NotImplementedException();
    }
}
