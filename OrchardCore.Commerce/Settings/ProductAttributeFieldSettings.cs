using System.Collections.Generic;
using OrchardCore.Commerce.Abstractions;

namespace OrchardCore.Commerce.Settings
{
    /// <summary>
    /// A base class for product attribute settings
    /// </summary>
    public abstract class ProductAttributeFieldSettings
    {
        /// <summary>
        /// The description text to display for this attribute in the product page
        /// </summary>
        public string Hint { get; set; }
    }

    /// <summary>
    /// A typed base class for product attribute settings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ProductAttributeFieldSettings<T> : ProductAttributeFieldSettings
    {
        /// <summary>
        /// The default value associated with this attribute
        /// </summary>
        public T DefaultValue { get; set; }
    }

    /// <summary>
    /// Settings for the Boolean product attribute
    /// </summary>
    public class BooleanProductAttributeFieldSettings : ProductAttributeFieldSettings<bool>
    {
        /// <summary>
        /// The text associated to the checkbox for this attribute in the product page
        /// </summary>
        public string Label { get; set; }
    }

    /// <summary>
    /// Settings for the numeric product attribute
    /// </summary>
    public class NumericProductAttributeFieldSettings : ProductAttributeFieldSettings<decimal?>
    {
        /// <summary>
        /// Whether a value is required
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// A hint to display when the input is empty
        /// </summary>
        public string Placeholder { get; set; }
        /// <summary>
        /// The number of digits after the decimal point
        /// </summary>
        public int DecimalPlaces { get; set; }
        /// <summary>
        /// The minimum value allowed
        /// </summary>
        public decimal? Minimum { get; set; }
        /// <summary>
        /// The maximum value allowed
        /// </summary>
        public decimal? Maximum { get; set; }
    }

    /// <summary>
    /// Settings for the text product attribute
    /// </summary>
    public class TextProductAttributeFieldSettings : ProductAttributeFieldSettings<string>, IPredefinedValuesProductAttributeFieldSettings
    {
        /// <summary>
        /// Whether a value is required
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// A hint to display when the input is empty
        /// </summary>
        public string Placeholder { get; set; }
        /// <summary>
        /// The set of suggested or allowed values
        /// </summary>
        public IEnumerable<object> PredefinedValues { get; set; }
        /// <summary>
        /// Whether values should be restricted to the set of predefined values
        /// </summary>
        public bool RestrictToPredefinedValues { get; set; }
        /// <summary>
        /// Whether multiple values can be selected
        /// </summary>
        public bool MultipleValues { get; set; }
    }
}
