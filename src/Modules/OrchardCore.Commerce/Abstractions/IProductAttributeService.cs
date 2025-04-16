using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A service for working with <see cref="ProductAttributeField"/>s.
/// </summary>
public interface IProductAttributeService
{
    /// <summary>
    /// Returns <see cref="ProductAttributeField"/>s and their settings of a given <paramref name="product"/>.
    /// </summary>
    Task<IEnumerable<ProductAttributeDescription>> GetProductAttributeFieldsAsync(ContentItem product);

    /// <summary>
    /// Returns the part and field definitions of the given <paramref name="type"/>'s specified <paramref name="attributeName"/>.
    /// </summary>
    (ContentTypePartDefinition PartDefinition, ContentPartFieldDefinition FieldDefinition)
        GetFieldDefinition(ContentTypeDefinition type, string attributeName);
}
