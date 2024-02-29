using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A service for displaying all fields of a content item.
/// </summary>
public interface IFieldsOnlyDisplayManager
{
    /// <summary>
    /// Returns a collection of display type names for each field in the given <paramref name="displayType"/>.
    /// </summary>
    Task<IEnumerable<string>> GetFieldShapeTypesAsync(
        ContentItem contentItem,
        string displayType = CommonContentDisplayTypes.Detail);

    /// <summary>
    /// Returns a collection of shapes for each field in the given <paramref name="displayType"/>.
    /// </summary>
    Task<IEnumerable<IShape>> DisplayFieldsAsync(
        ContentItem contentItem,
        string displayType = CommonContentDisplayTypes.Detail);

    /// <summary>
    /// Returns a collection of URLs that either edit an existing template or create a new one if none exists for each
    /// field in the given <paramref name="displayType"/>.
    /// </summary>
    Task<IEnumerable<(string ShapeType, Uri Url, bool IsNew)>> GetFieldTemplateEditorUrlsAsync(
        ContentItem contentItem,
        string displayType = CommonContentDisplayTypes.Detail);
}
