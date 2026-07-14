using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Services;

public class SkuService : ISkuService
{
    private readonly IEnumerable<ISkuGenerator> _skuGenerators;
    public SkuService(IEnumerable<ISkuGenerator> skuGenerators) => _skuGenerators = skuGenerators;

    public virtual bool IsReadOnly() => _skuGenerators.HighestPriority() is { IsManualAllowed: false };
    public virtual bool TryGetGenerator(ProductPart part, out ISkuGenerator skuGenerator)
    {
        var generator = _skuGenerators.HighestPriority();

        // No generator available
        if (generator == null)
        {
            skuGenerator = null;
            return false;
        }

        // Condition for allowing the generator
        bool canGenerate = string.IsNullOrWhiteSpace(part.Sku) && !generator.IsManualAllowed;

        skuGenerator = canGenerate ? generator : null;
        return canGenerate;
    }
}
