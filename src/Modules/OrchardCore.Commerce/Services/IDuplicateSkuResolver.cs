using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A provider for determining if existing items with the same SKU are a problem.
/// </summary>
public interface IDuplicateSkuResolver
{
    /// <summary>
    /// Modifies the provided <paramref name="otherProducts"/> list if needed. Implementations can remove duplicates
    /// which are considered justified.
    /// </summary>
    /// <param name="current">The product currently being evaluated.</param>
    /// <param name="otherProducts">A list of existing duplicates with the same SKU.</param>
    /// <returns>The list of unresolved duplicates.</returns>
    Task<IList<ContentItem>> UpdateDuplicatesListAsync(ContentItem current, IList<ContentItem> otherProducts);
}
