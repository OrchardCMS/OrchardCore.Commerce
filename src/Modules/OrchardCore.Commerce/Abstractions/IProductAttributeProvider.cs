using OrchardCore.ContentManagement.Metadata.Models;
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
    /// Parses the provided JSON-serialized data.
    /// </summary>
    IProductAttributeValue CreateFromJsonElement(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        JsonElement value);
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
