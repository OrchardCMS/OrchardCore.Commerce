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

    /// <summary>
    /// A Boolean product attribute
    /// </summary>
    public class BooleanProductAttributeField : ProductAttributeField { }

    /// <summary>
    /// A numeric product attribute
    /// </summary>
    public class NumericProductAttributeField : ProductAttributeField { }

    /// <summary>
    /// A text product attribute, that may also have predefined values and be used as enumerations or flags
    /// </summary>
    public class TextProductAttributeField : ProductAttributeField { }
}
