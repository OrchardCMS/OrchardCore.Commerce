using Newtonsoft.Json;
using OrchardCore.Commerce.Serialization;
using System;
using System.Globalization;
using System.Linq;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// A specific value from a product attribute field.
/// </summary>
[JsonConverter(typeof(ProductAttributeValueConverter))]
public interface IProductAttributeValue
{
    /// <summary>
    /// Gets the name of the attribute.
    /// </summary>
    string AttributeName { get; }

    /// <summary>
    /// Gets the value of the attribute without a known type.
    /// </summary>
    object UntypedValue { get; }

    /// <summary>
    /// Gets the first part of the <see cref="AttributeName"/>.
    /// </summary>
    public string Label => AttributeName?.Split('.').FirstOrDefault();

    /// <summary>
    /// Gets the second part of the <see cref="AttributeName"/>.
    /// </summary>
    public string PartName => AttributeName?.Split('.').Skip(1).FirstOrDefault();

    /// <summary>
    /// Returns the user-facing string for this attribute value.
    /// </summary>
    string Display(CultureInfo culture = null);
}

/// <summary>
/// A specific value from a product attribute field of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of <see cref="Value"/>.</typeparam>
[JsonConverter(typeof(ProductAttributeValueConverter))]
public interface IProductAttributeValue<T> : IProductAttributeValue, IEquatable<IProductAttributeValue<T>>
{
    /// <summary>
    /// Gets the value of the attribute with a known type of <typeparamref name="T"/>.
    /// </summary>
    T Value { get; }
}
