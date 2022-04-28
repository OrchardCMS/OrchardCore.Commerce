using OrchardCore.Commerce.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OrchardCore.Commerce.ProductAttributeValues;

public class TextProductAttributeValue
    : BaseProductAttributeValue<IEnumerable<string>>, IPredefinedValuesProductAttributeValue<string>
{
    public TextProductAttributeValue(string attributeName, IEnumerable<string> values)
        : base(attributeName, values)
    {
    }

    public TextProductAttributeValue(string attributeName, params string[] values)
        : this(attributeName, (IEnumerable<string>)values)
    {
    }

    public override string Display(CultureInfo culture = null) =>
        Value?.FirstOrDefault() is null
            ? string.Empty
            : FieldName + ": " + string.Join(", ", Value);

    public override bool Equals(object obj) => base.Equals(obj);

    public override bool Equals(IProductAttributeValue<IEnumerable<string>> other) =>
        other?.Value == null || !other.Value.Any()
            ? Value == null || !Value.Any()
            : Value != null &&
              Value.Any() &&
              AttributeName == other.AttributeName &&
              new HashSet<string>(Value).SetEquals(other.Value);

    public override int GetHashCode() =>
        Value is null ? 1.GetHashCode() : Value.Aggregate(1.GetHashCode(), (code, val) => (code, val).GetHashCode());

    public override string ToString() => AttributeName + ": " + string.Join(", ", Value);

    public object UntypedPredefinedValue => PredefinedValue;

    public string PredefinedValue => Value?.FirstOrDefault();
}
