using OrchardCore.Commerce.Fields;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service for working with <see cref="ProductAttributeField"/>s.
/// </summary>
public interface IProductAttributeService
{
    /// <summary>
    /// Returns <see cref="ProductAttributeField"/>s and their settings of a given <paramref name="product"/>.
    /// </summary>
    IEnumerable<ProductAttributeDescription> GetProductAttributeFields(ContentItem product);
}
