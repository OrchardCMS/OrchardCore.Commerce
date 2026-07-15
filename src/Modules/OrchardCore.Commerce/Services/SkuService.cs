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
            part.Sku = skuBefore;
            return;
        }

        part.Sku = part.Sku.ToUpperInvariant();
    }

    /// <summary>
    /// If the user didn't fill in the SKU even though manual entry is allowed, the system should generate one. (true || false).
    /// If the user manually edited the HTML before submitting the form, but manual entry is not allowed,
    /// the system should overwrite the submitted value with a generated one(false || true).
    /// Keep virtual for different implementations to override the default behavior.
    /// </summary>
    /// <param name="part"><see cref="ProductPart"/>.</param>
    /// <param name="skuGenerator">returns the <see cref="ISkuGenerator"/>.</param>
    /// <returns>True when an <see cref="ISkuGenerator"/> should be used; otherwise, false.</returns>
    public virtual bool TryGetGenerator(ProductPart part, out ISkuGenerator skuGenerator)
    {
        skuGenerator = _skuGenerators.HighestPriority();

        if (skuGenerator == null) return false;

        return string.IsNullOrWhiteSpace(part.Sku) || !skuGenerator.IsManualAllowed;
    }
}
