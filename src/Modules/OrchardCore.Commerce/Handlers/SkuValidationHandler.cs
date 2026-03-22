using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Handlers;

public class SkuValidationHandler : ContentPartHandler<ProductPart>
{
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IEnumerable<IDuplicateSkuResolver> _duplicateSkuResolvers;
    private readonly IEnumerable<ISkuGenerator> _skuGenerators;
    private readonly IStringLocalizer<SkuValidationHandler> T;

    public SkuValidationHandler(
        ISession session,
        IUpdateModelAccessor updateModelAccessor,
        IEnumerable<IDuplicateSkuResolver> duplicateSkuResolvers,
        IEnumerable<ISkuGenerator> skuGenerators,
        IStringLocalizer<SkuValidationHandler> stringLocalizer)
    {
        _session = session;
        _updateModelAccessor = updateModelAccessor;
        _duplicateSkuResolvers = duplicateSkuResolvers;
        _skuGenerators = skuGenerators;
        T = stringLocalizer;
    }

    public override async Task CreatingAsync(CreateContentContext context, ProductPart part)
    {
        // If we have an SKU generator and the SKU is either empty or it must not be manually filled, then overwrite it
        // with the generated value. 
        if (_skuGenerators.HighestPriority() is { } generator &&
            (string.IsNullOrWhiteSpace(part.Sku) || !generator.IsManualAllowed))
        {
            part.Sku = await generator.GenerateSkuAsync(part.ContentItem);
        }
        
        await CreatingOrUpdatingAsync(part);
    }

    public override Task UpdatingAsync(UpdateContentContext context, ProductPart part) =>
        CreatingOrUpdatingAsync(part);

    private async Task CreatingOrUpdatingAsync(ProductPart part)
    {
        if (string.IsNullOrWhiteSpace(part.Sku))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(nameof(part.Sku), T["SKU must not be empty."]);
            return;
        }

        var alreadyExisting = (await _session
                .Query<ContentItem, ProductPartIndex>(index =>
                    index.Sku == part.Sku &&
                    index.ContentItemId != part.ContentItem.ContentItemId)
                .ListAsync())
            .AsList();

        var resolvers = _duplicateSkuResolvers.AsList();
        for (var i = 0; i < resolvers.Count && alreadyExisting.Any(); i++)
        {
            alreadyExisting =
                await resolvers[i].UpdateDuplicatesListAsync(part.ContentItem, alreadyExisting) ??
                Array.Empty<ContentItem>();
        }

        if (alreadyExisting.Any())
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(part.Sku),
                T["SKU must be unique. A product with the given SKU already exists."]);
        }
    }
}
