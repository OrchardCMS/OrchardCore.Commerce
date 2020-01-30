using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.ProductAttributeValues
{

    public class TextProductAttributeValue : BaseProductAttributeValue<IEnumerable<string>>, IPredefinedValuesProductAttributeValue<string>
    {
        public TextProductAttributeValue(string attributeName, IEnumerable<string> value)
            : base(attributeName, value) { }

        public TextProductAttributeValue(string attributeName, params string[] values)
            : this(attributeName, (IEnumerable<string>)values) { }

        public override string Display(CultureInfo culture = null)
            => Value is null || !Value.Any() || Value.First() is null ? "" : FieldName + ": " + String.Join(", ", Value);

        public override bool Equals(IProductAttributeValue<IEnumerable<string>> other)
            => other == null || other.Value == null || !other.Value.Any() ? Value == null || !Value.Any()
            : Value == null || !Value.Any() || AttributeName != other.AttributeName ? false
            : new HashSet<string>(Value).SetEquals(other.Value);

        public override int GetHashCode()
            => Value is null ? 1.GetHashCode() : Value.Aggregate(1.GetHashCode(), (code, val) => (code, val).GetHashCode());

        public override string ToString() => AttributeName + ": " + String.Join(", ", Value);

        public object UntypedPredefinedValue => PredefinedValue;

        public string PredefinedValue => Value?.FirstOrDefault();
    }
}
