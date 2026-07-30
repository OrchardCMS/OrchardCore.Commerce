using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Handlers;

public class SkuValidationHandler : ContentPartHandler<ProductPart>
{
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IEnumerable<IDuplicateSkuResolver> _duplicateSkuResolvers;
    private readonly ISkuService _skuService;
    private readonly IStringLocalizer<SkuValidationHandler> T;
    private readonly IProductAttributeService _productAttributeService;

    public SkuValidationHandler(
        ISession session,
        IUpdateModelAccessor updateModelAccessor,
        IEnumerable<IDuplicateSkuResolver> duplicateSkuResolvers,
        ISkuService skuService,
        IProductAttributeService productAttributeService,
        IStringLocalizer<SkuValidationHandler> stringLocalizer)
    {
        _session = session;
        _updateModelAccessor = updateModelAccessor;
        _duplicateSkuResolvers = duplicateSkuResolvers;
        _skuService = skuService;
        T = stringLocalizer;
        _productAttributeService = productAttributeService;
    }

    public override async Task CreatingAsync(CreateContentContext context, ProductPart part)
    {
        var skuBefore = part.Sku ?? string.Empty;

        // If we have an SKU generator and the SKU is either empty or it must not be manually filled, then overwrite it
        // with the generated value.
        if (_skuService.TryGetGenerator(part, out var generator))
        {
            part.Sku = await generator.GenerateSkuAsync(part.ContentItem);
            part.ContentItem.Apply(part);

            _productAttributeService.UpdateCanBeBoughtForProductAttributeField(part, skuBefore);
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

        var alreadyExisting = await _session
            .Query<ContentItem, ProductPartIndex>(index =>
                index.Sku == part.Sku &&
                index.ContentItemId != part.ContentItem.ContentItemId)
            .ListReadOnlyAsync();

        var resolvers = _duplicateSkuResolvers.AsList();
        for (var i = 0; i < resolvers.Count && alreadyExisting.Count > 0; i++)
        {
            alreadyExisting = await resolvers[i].UpdateDuplicatesListAsync(part.ContentItem, alreadyExisting) ?? [];
        }

        if (alreadyExisting.Count > 0)
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                nameof(part.Sku),
                T["SKU must be unique. A product with the given SKU already exists."]);
        }
    }
}
