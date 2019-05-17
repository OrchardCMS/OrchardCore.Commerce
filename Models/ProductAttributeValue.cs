using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Models
{
    public class BaseProductAttributeValue<T> : IProductAttributeValue<T>
    {
        public BaseProductAttributeValue(string attributeName, T value)
        {
            AttributeName = attributeName;
            Value = value;
        }

        public virtual T Value { get; }

        public virtual string AttributeName { get; }

        public object UntypedValue => Value;

        public virtual string Display(CultureInfo culture = null)
            => AttributeName + ": " + Convert.ToString(Value, culture ?? CultureInfo.InvariantCulture);

        public virtual bool Equals(IProductAttributeValue<T> other)
            => other != null
                && AttributeName == other.AttributeName
                && ((Value == null && other.Value == null) || Value.Equals(other.Value));

        public override bool Equals(object obj) => obj != null && obj is IProductAttributeValue<T> other && Equals(other);

        public override int GetHashCode() => (AttributeName, Value).GetHashCode();

        public override string ToString() => AttributeName + ": " + Value;
    }

    public class BooleanProductAttributeValue : BaseProductAttributeValue<bool>
    {
        public BooleanProductAttributeValue(string attributeName, bool value)
            : base(attributeName, value) { }
    }

    public class NumericProductAttributeValue : BaseProductAttributeValue<decimal?>
    {
        public NumericProductAttributeValue(string attributeName, decimal? value)
            : base(attributeName, value) { }
    }

    public class TextProductAttributeValue : BaseProductAttributeValue<IEnumerable<string>>
    {
        public TextProductAttributeValue(string attributeName, IEnumerable<string> value)
            : base(attributeName, value) { }

        public TextProductAttributeValue(string attributeName, params string[] values)
            : this(attributeName, (IEnumerable<string>)values) { }

        public override string Display(CultureInfo culture = null)
            => AttributeName + ": " + String.Join(", ", Value);

        public override bool Equals(IProductAttributeValue<IEnumerable<string>> other)
            => other == null || other.Value == null || !other.Value.Any() ? Value == null || !Value.Any()
            : Value == null || !Value.Any() || AttributeName != other.AttributeName ? false
            : new HashSet<string>(Value).SetEquals(other.Value);

        public override string ToString() => AttributeName + ": " + String.Join(", ", Value);
    }
}
