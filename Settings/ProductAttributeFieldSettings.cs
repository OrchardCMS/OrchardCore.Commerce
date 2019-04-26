using System.Collections.Generic;

namespace OrchardCore.Commerce.Settings
{
    public abstract class ProductAttributeFieldSettings
    {
        public string Hint { get; set; }
    }

    public abstract class ProductAttributeFieldSettings<T> : ProductAttributeFieldSettings
    {
        public T DefaultValue { get; set; }
    }

    public class BooleanProductAttributeFieldSettings : ProductAttributeFieldSettings<bool>
    {
        public string Label { get; set; }
    }

    public class NumericProductAttributeFieldSettings : ProductAttributeFieldSettings<decimal?>
    {
        public bool Required { get; set; }
        public string Placeholder { get; set; }
        public int DecimalPlaces { get; set; }
        public decimal? Minimum { get; set; }
        public decimal? Maximum { get; set; }
    }

    public class TextProductAttributeFieldSettings : ProductAttributeFieldSettings<string>
    {
        public bool Required { get; set; }
        public string Placeholder { get; set; }
        public string[] PredefinedValues { get; set; }
        public bool RestrictToPredefinedValues { get; set; }
        public bool MultipleValues { get; set; }
    }
}
