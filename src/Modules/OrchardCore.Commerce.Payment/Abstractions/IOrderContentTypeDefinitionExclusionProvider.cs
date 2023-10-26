using OrchardCore.Commerce.Drivers;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Abstractions;

/// <summary>
/// An extension point to specify which fields to exclude in <see
/// cref="OrderContentTypeDefinitionDisplayDriver"/>.
/// </summary>
public interface IOrderContentTypeDefinitionExclusionProvider
{
    /// <summary>
    /// Returns the list of field shape types to exclude when editing <see
    /// cref="OrderContentTypeDefinitionDisplayDriver"/>.
    /// </summary>
    IEnumerable<string> GetExcludedShapes(IEnumerable<(string ShapeType, Uri Url, bool IsNew)> templateLinks);
}
