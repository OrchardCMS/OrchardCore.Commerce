using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Services
{
    public class ProductAttributeParseService : IProductAttributeParseService
    {
        public IProductAttributeValue Parse(ContentPartFieldDefinition attributeFieldDefinition, string value)
        {
            string attributeFieldTypeName = attributeFieldDefinition.FieldDefinition.Name;
            string name = attributeFieldDefinition.Name;
            switch(attributeFieldTypeName)
            {
                case nameof(BooleanProductAttributeField):
                    return new BooleanProductAttributeValue(name, bool.Parse(value));
                case nameof(NumericProductAttributeField):
                    if (decimal.TryParse(value, out decimal decimalValue))
                    {
                        return new NumericProductAttributeValue(name, decimalValue);
                    }
                    return new NumericProductAttributeValue(name, null);
                case nameof(TextProductAttributeField):
                    // TODO: use settings to validate the value, and parse differently if multiple values are allowed.
                    return new TextProductAttributeValue(name, value?.Split(','));
                default:
                    return null;
            }
        }
    }
}
