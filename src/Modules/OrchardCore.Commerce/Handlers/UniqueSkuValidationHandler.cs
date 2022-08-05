using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Handlers;

public class UniqueSkuValidationHandler : ContentPartHandler<ProductPart>
{
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public UniqueSkuValidationHandler(
        IContentManager contentManager,
        ISession session,
        IUpdateModelAccessor updateModelAccessor)
    {
        _contentManager = contentManager;
        _session = session;
        _updateModelAccessor = updateModelAccessor;
    }

    public override async Task UpdatedAsync(UpdateContentContext context, ProductPart instance)
    {
        var products = (await _session
            .QueryContentItem(PublicationStatus.Published)
            .ListAsync()).AsList();

        var isNotUnique = products.Any(sku => sku.As<ProductPart>()?.Sku == instance.Sku);

        if (isNotUnique)
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(nameof(instance.Sku), "SKU must be unique.");
        }
    }
}
