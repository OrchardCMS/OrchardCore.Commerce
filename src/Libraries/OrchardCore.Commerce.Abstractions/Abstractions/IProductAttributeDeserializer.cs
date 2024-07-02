using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace OrchardCore.Commerce.Abstractions.Abstractions;

/// <summary>
/// Deserializes the attribute of the type <see cref="AttributeTypeName"/>.
/// </summary>
public interface IProductAttributeDeserializer
{
    // Not necessary to document as they are not externally accessible.
#pragma warning disable SA1600 // Elements should be documented.
    internal static readonly Dictionary<string, IProductAttributeDeserializer> Deserializers =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly object _lock = new();
#pragma warning restore SA1600

    /// <summary>
    /// Gets the attribute name used to identify this deserializer.
    /// </summary>
    string AttributeTypeName { get; }

    /// <summary>
    /// Deserializes using <c>System.Text.Json</c>.
    /// </summary>
    IProductAttributeValue Deserialize(string attributeName, JsonObject attribute);

    /// <summary>
    /// Registers serializers used to deserialize product attributes.
    /// </summary>
    public static void AddSerializers(params IProductAttributeDeserializer[] deserializers)
    {
        lock (_lock)
        {
            foreach (var deserializer in deserializers)
            {
                Deserializers[deserializer.AttributeTypeName] = deserializer;
            }
        }
    }
}
