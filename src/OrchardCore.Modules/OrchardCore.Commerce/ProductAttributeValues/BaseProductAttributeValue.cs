using System;
using System.Globalization;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.ProductAttributeValues
{
    public class BaseProductAttributeValue<T> : IProductAttributeValue<T>
    {
        public BaseProductAttributeValue(string attributeName, T value)
        {
            AttributeName = attributeName;
            Value = value;
        }

        public virtual T Value { get; }

        public virtual string AttributeName { get; protected set; }

        public object UntypedValue => Value;

        public virtual string Display(CultureInfo culture = null)
            => FieldName + ": " + Convert.ToString(Value, culture ?? CultureInfo.InvariantCulture);

        public virtual bool Equals(IProductAttributeValue<T> other)
            => other != null
                && AttributeName == other.AttributeName
                && (Value == null && other.Value == null || Value.Equals(other.Value));

        public override bool Equals(object obj) => obj != null && obj is IProductAttributeValue<T> other && Equals(other);

        public override int GetHashCode() => (AttributeName, Value).GetHashCode();

        public override string ToString() => AttributeName + ": " + Value;

        public string FieldName
        {
            get
            {
                int dot = AttributeName.IndexOf('.');
                if (dot == -1 || dot + 1 == AttributeName.Length) return AttributeName;
                return AttributeName.Substring(dot + 1);
            }
        }
    }
}
