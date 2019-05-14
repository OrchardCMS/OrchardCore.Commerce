using System;
using System.Globalization;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductAttributeValue
    {
        string AttributeName { get; }
        object UntypedValue { get; }

        string Display(CultureInfo culture = null);
    }
    public interface IProductAttributeValue<T> : IProductAttributeValue, IEquatable<IProductAttributeValue<T>>
    {
        T Value { get; }
    }
}