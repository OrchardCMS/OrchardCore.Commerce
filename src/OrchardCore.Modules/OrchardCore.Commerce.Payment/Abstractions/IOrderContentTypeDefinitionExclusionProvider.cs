using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Abstractions;

/// <summary>
/// An extension point to specify which fields to exclude in <c>OrderContentTypeDefinitionDisplayDriver</c>.
/// </summary>
public interface IOrderContentTypeDefinitionExclusionProvider
{
    /// <summary>
    /// Returns the list of field shape types to exclude when editing <c>OrderContentTypeDefinitionDisplayDriver</c>.
    /// </summary>
    IEnumerable<string> GetExcludedShapes(IEnumerable<(string ShapeType, Uri Url, bool IsNew)> templateLinks);
}
