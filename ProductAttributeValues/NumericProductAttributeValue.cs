namespace OrchardCore.Commerce.ProductAttributeValues
{
    public class NumericProductAttributeValue : BaseProductAttributeValue<decimal?>
    {
        public NumericProductAttributeValue(string attributeName, decimal? value)
            : base(attributeName, value) { }
    }
}
