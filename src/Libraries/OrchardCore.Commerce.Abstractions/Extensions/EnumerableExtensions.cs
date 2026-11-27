using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.ProductAttributeValues;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Abstractions.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Returns a value indicating whether any of the attributes need post-processing.
    /// </summary>
    public static bool HasRawAttributes(this IEnumerable<IProductAttributeValue> attributes) =>
        attributes.Any(attribute => attribute is RawProductAttributeValue);
}
