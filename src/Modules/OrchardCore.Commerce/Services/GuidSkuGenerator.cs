using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.ContentManagement;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class GuidSkuGenerator : ISkuGenerator
{
    public int Priority => int.MinValue;

    public Task<string> GenerateSkuAsync(ContentItem contentItem) =>
        Task.FromResult(Guid.NewGuid().ToString("N"));
}
