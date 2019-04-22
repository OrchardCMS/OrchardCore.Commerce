using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Fields
{
    /// <summary>
    /// Adds the ability for a product to be modified with a set of attributes, in particular when
    /// added to a shopping cart.
    /// Examples of attributes can be shirt sizes (S, M, L, XL), dimensions, etc.
    /// </summary>
    public abstract class ProductAttributeField : ContentField
    {
        public ProductAttributeField() { }
    }

    public class BooleanProductAttributeField : ProductAttributeField { }

    public class NumericProductAttributeField : ProductAttributeField { }

    public class TextProductAttributeField : ProductAttributeField { }
}
