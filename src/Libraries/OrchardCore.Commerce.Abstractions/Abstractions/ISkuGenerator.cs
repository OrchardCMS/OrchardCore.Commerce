using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions.Abstractions;

/// <summary>
/// A service that can provide an alternative to manually filling out the product part's SKU field.
/// </summary>
public interface ISkuGenerator
{
    /// <summary>
    /// Gets the sorting order when selecting SKU generator. If multiple generators are registered, the one with the
    /// highest priority is used.
    /// </summary>
    public int Priority => 0;

    /// <summary>
    /// Gets a value indicating whether this generator allows user fill-in. If <see langword="false"/>, the SKU field is
    /// hidden in the product editor. If <see langword="true"/>, the generator is only used when the product is
    /// published with an empty SKU.  
    /// </summary>
    public bool IsManualAllowed => false;

    /// <summary>
    /// Returns the SKU for the provided <paramref name="contentItem"/>.
    /// </summary>
    /// <param name="contentItem">
    /// The content item whose product SKU is to be generated. Must not be modified by this method.
    /// </param>
    public Task<string> GenerateSkuAsync(ContentItem contentItem);
}
