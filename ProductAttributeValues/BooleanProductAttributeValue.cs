using System.Globalization;

namespace OrchardCore.Commerce.ProductAttributeValues
{
    public class BooleanProductAttributeValue : BaseProductAttributeValue<bool>
    {
        public BooleanProductAttributeValue(string attributeName, bool value)
            : base(attributeName, value) { }

        public override string Display(CultureInfo culture = null)
            => Value ? FieldName : "";
    }
}
