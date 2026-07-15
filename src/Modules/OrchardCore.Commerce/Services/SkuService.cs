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

    public virtual void Update(ProductPart part, string skuBefore)
    {
        if (!string.IsNullOrEmpty(skuBefore) && IsReadOnly())
        {
            // If the SKU is read-only then editing should not be possible, but here we undo any POST trickery just in case.
            part.Sku = skuBefore;
            return;
        }

        part.Sku = part.Sku.ToUpperInvariant();
    }

    public virtual bool TryGetGenerator(ProductPart part, out ISkuGenerator skuGenerator)
    {
        skuGenerator = _skuGenerators.HighestPriority();

        // No generator is available.
        if (skuGenerator == null) return false;

        // Condition for allowing the generator.
        return string.IsNullOrWhiteSpace(part.Sku) && !skuGenerator.IsManualAllowed;
    }
}
