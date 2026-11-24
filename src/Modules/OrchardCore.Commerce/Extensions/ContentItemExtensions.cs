using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrchardCore.Commerce.Extensions;

public static class ContentItemExtensions
{
    /// <summary>
    /// Returns all <see cref="ContentField"/> items in each <see cref="ContentPart"/> of <paramref name="contentItem"/>
    /// that inherit from <typeparamref name="TResult"/>.
    /// </summary>
    public static IEnumerable<TResult> GetFields<TResult>(this ContentItem contentItem) =>
        contentItem.OfType<ContentPart>().SelectMany(part => part.UnconstrainedOfType<ContentField, TResult>());

    /// <summary>
    /// Returns all unique content field types (keyed by <see cref="MemberInfo.Name"/>) that inherit from <typeparamref
    /// name="TAncestor"/>.
    /// </summary>
    public static IDictionary<string, Type> GetFieldTypes<TAncestor>(this ContentItem contentItem) =>
        contentItem
            .GetFields<TAncestor>()
            .Select(field => field.GetType())
            .Distinct()
            .ToDictionary(type => type.Name);

    private static IEnumerable<TResult> UnconstrainedOfType<TElement, TResult>(this ContentElement contentElement)
        where TElement : ContentElement =>
        contentElement.OfType<TElement>().OfType<TResult>();
}
