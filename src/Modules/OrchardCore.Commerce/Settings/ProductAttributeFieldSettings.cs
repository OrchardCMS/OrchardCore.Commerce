using Lombiq.HelpfulLibraries.Common.Utilities;
using OrchardCore.Commerce.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Settings;

/// <summary>
/// A base class for product attribute settings.
/// </summary>
public abstract class ProductAttributeFieldSettings : ICopier<ProductAttributeFieldSettings>
{
    /// <summary>
    /// Gets or sets the description text to display for this attribute in the product page.
    /// </summary>
    public string Hint { get; set; }

    public void CopyTo(ProductAttributeFieldSettings target) => target.Hint = Hint;
}

/// <summary>
/// A typed base class for product attribute settings.
/// </summary>
public abstract class ProductAttributeFieldSettings<T> : ProductAttributeFieldSettings, ICopier<ProductAttributeFieldSettings<T>>
{
    /// <summary>
    /// Gets or sets the default value associated with this attribute.
    /// </summary>
    public T DefaultValue { get; set; }

    public void CopyTo(ProductAttributeFieldSettings<T> target)
    {
        ((ICopier<ProductAttributeFieldSettings>)this).CopyTo(target);
        target.DefaultValue = DefaultValue;
    }
}

/// <summary>
/// Settings for the Boolean product attribute.
/// </summary>
public class BooleanProductAttributeFieldSettings : ProductAttributeFieldSettings<bool>, ICopier<BooleanProductAttributeFieldSettings>
{
    /// <summary>
    /// Gets or sets the text associated to the checkbox for this attribute in the product page.
    /// </summary>
    public string Label { get; set; }

    public void CopyTo(BooleanProductAttributeFieldSettings target)
    {
        ((ProductAttributeFieldSettings<bool>)this).CopyTo(target);
        target.Label = Label;
    }
}

/// <summary>
/// Settings for the numeric product attribute.
/// </summary>
public class NumericProductAttributeFieldSettings : ProductAttributeFieldSettings<decimal?>, ICopier<NumericProductAttributeFieldSettings>
{
    /// <summary>
    /// Gets or sets a value indicating whether a value is required.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the hint to display when the input is empty.
    /// </summary>
    public string Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the number of digits after the decimal point.
    /// </summary>
    public int DecimalPlaces { get; set; }

    /// <summary>
    /// Gets or sets the minimum value allowed.
    /// </summary>
    public decimal? Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum value allowed.
    /// </summary>
    public decimal? Maximum { get; set; }

    public void CopyTo(NumericProductAttributeFieldSettings target)
    {
        ((ProductAttributeFieldSettings<decimal?>)this).CopyTo(target);

        target.Required = Required;
        target.Placeholder = Placeholder;
        target.DecimalPlaces = DecimalPlaces;
        target.Minimum = Minimum;
        target.Maximum = Maximum;
    }
}

/// <summary>
/// Settings for the text product attribute.
/// </summary>
public class TextProductAttributeFieldSettings
    : ProductAttributeFieldSettings<string>, IPredefinedValuesProductAttributeFieldSettings, ICopier<TextProductAttributeFieldSettings>
{
    /// <summary>
    /// Gets or sets a value indicating whether a value is required.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the hint to display when the input is empty.
    /// </summary>
    public string Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the set of suggested or allowed values.
    /// </summary>
    public IEnumerable<object> PredefinedValues { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets a value indicating whether values should be restricted to the set of predefined values.
    /// </summary>
    public bool RestrictToPredefinedValues { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple values can be selected.
    /// </summary>
    public bool MultipleValues { get; set; }

    public void CopyTo(TextProductAttributeFieldSettings target)
    {
        ((ProductAttributeFieldSettings<string>)this).CopyTo(target);

        target.Required = Required;
        target.Placeholder = Placeholder;
        target.PredefinedValues = PredefinedValues;
        target.RestrictToPredefinedValues = RestrictToPredefinedValues;
        target.MultipleValues = MultipleValues;
    }
}
