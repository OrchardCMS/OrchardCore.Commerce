using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A provider for retrieving <see cref="IProductAttributeValue"/> from an attribute field.
/// </summary>
public interface IProductAttributeProvider
{
    /// <summary>
    /// Parses the provided strings.
    /// </summary>
    IProductAttributeValue Parse(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        string[] value);

    /// <summary>
    /// Returns the given set of <paramref name="attributes"/> in dictionary form, keyed by attribute type.
    /// </summary>
    IDictionary<string, IDictionary<string, string>> GetSelectedAttributes(ISet<IProductAttributeValue> attributes);

    /// <summary>
    /// Procures a dictionary of <paramref name="selectedAttributes"/> using the given <paramref name="productPart"/>
    /// and <paramref name="attributesList"/>.
    /// </summary>
    void HandleSelectedAttributes(
        IDictionary<string, IDictionary<string, string>> selectedAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList);

    /// <summary>
    /// Parses the provided attribute value.
    /// </summary>
    IProductAttributeValue CreateFromValue(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        object value) =>
#pragma warning disable CS0618 // Type or member is obsolete. Backwards compatibility.
        CreateFromJsonElement(
            partDefinition,
            attributeFieldDefinition,
            value is JsonElement element ? element : default);
#pragma warning restore CS0618 // Type or member is obsolete. Backwards compatibility.

    /// <summary>
    /// Parses the provided JSON-serialized data.
    /// </summary>
    [Obsolete($"Use {nameof(CreateFromValue)} instead.")]
    IProductAttributeValue CreateFromJsonElement(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        JsonElement value) =>
        throw new NotSupportedException($"This attribute provider doesn't support {nameof(JsonElement)}.");
}

public static class ProductAttributeProviderExtensions
{
    public static IProductAttributeValue Parse(
        this IProductAttributeProvider provider,
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        string value) =>
        provider.Parse(partDefinition, attributeFieldDefinition, new[] { value });
}
