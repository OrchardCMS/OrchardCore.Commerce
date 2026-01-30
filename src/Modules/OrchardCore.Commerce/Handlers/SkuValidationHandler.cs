using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Handlers;

public class SkuValidationHandler : CreatingOrUpdatingPartHandler<ProductPart>
{
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IEnumerable<IDuplicateSkuResolver> _duplicateSkuResolvers;
    private readonly IStringLocalizer<SkuValidationHandler> T;

    public SkuValidationHandler(
        ISession session,
        IUpdateModelAccessor updateModelAccessor,
        IEnumerable<IDuplicateSkuResolver> duplicateSkuResolvers,
        IStringLocalizer<SkuValidationHandler> stringLocalizer)
    {
        _session = session;
        _updateModelAccessor = updateModelAccessor;
        _duplicateSkuResolvers = duplicateSkuResolvers;
        T = stringLocalizer;
    }

    protected override async Task CreatingOrUpdatingAsync(ProductPart part)
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
        for (var i = 0; i < resolvers.Count && alreadyExisting?.Any() == true; i++)
        {
            alreadyExisting =
                await resolvers[i].UpdateDuplicatesListAsync(part.ContentItem, alreadyExisting) ??
                Array.Empty<ContentItem>();
        }

        if (alreadyExisting?.Any() == true)
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(part.Sku),
                T["SKU must be unique. A product with the given SKU already exists."]);
        }
    }
}
