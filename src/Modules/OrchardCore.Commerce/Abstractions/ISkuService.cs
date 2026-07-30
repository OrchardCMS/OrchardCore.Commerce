using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Models;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Provides methods for working with SKUs and SKU generators.
/// </summary>
public interface ISkuService
{
    /// <summary>
    /// if there is a SKU generator registered and it does not allow manual fill-in, then the SKU field is read-only.
    /// </summary>
    /// <returns>True if it is read only, otherwise, false.</returns>
    bool IsReadOnly();

    /// <summary>
    /// Update the SKU of a <see cref="ProductPart"/> based on the registered SKU generator, if any.
    /// </summary>
    /// <param name="part"><see cref="ProductPart"/>.</param>
    /// <param name="skuBefore">Previous SKU.</param>
    void Update(ProductPart part, string skuBefore);

    /// <summary>
    /// Judges whether a <see cref="ProductPart"/> has to be a SKU generator associated with it, and if so, returns true with the generator.
    /// </summary>
    /// <param name="part">The <see cref="ProductPart"/>.</param>
    /// <param name="skuGenerator">returns the <see cref="ISkuGenerator"/>.</param>
    /// <returns>
    /// True when an <see cref="ISkuGenerator"/> should be used; otherwise, false.
    /// </returns>
    bool TryGetGenerator(ProductPart part, out ISkuGenerator skuGenerator);
}
