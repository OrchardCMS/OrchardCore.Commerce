using System;
using System.Linq;
using System.Text.Json;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Services
{
    public class ProductAttributeProvider : IProductAttributeProvider
    {
        public IProductAttributeValue CreateFromJsonElement(
            ContentTypePartDefinition partDefinition,
            ContentPartFieldDefinition attributeFieldDefinition,
            JsonElement value)
        {
            string attributeFieldTypeName = attributeFieldDefinition.FieldDefinition.Name;
            string name = partDefinition.Name + "." + attributeFieldDefinition.Name;
            switch (attributeFieldTypeName)
            {
                case nameof(BooleanProductAttributeField):
                    return new BooleanProductAttributeValue(name, value.GetBoolean());
                case nameof(NumericProductAttributeField):
                    if (value.TryGetDecimal(out decimal decimalValue))
                    {
                        return new NumericProductAttributeValue(name, decimalValue);
                    }
                    return new NumericProductAttributeValue(name, null);
                case nameof(TextProductAttributeField):
                    switch (value.ValueKind)
                    {
                        case JsonValueKind.String:
                            return new TextProductAttributeValue(name, value.GetString());
                        case JsonValueKind.Array:
                            return new TextProductAttributeValue(name, value.EnumerateArray().Select(el => el.GetString()));
                        default:
                            return new TextProductAttributeValue(name, null);
                    }
                default:
                    return null;
            }
        }

        public IProductAttributeValue Parse(
            ContentTypePartDefinition partDefinition,
            ContentPartFieldDefinition attributeFieldDefinition,
            string[] value)
        {
            string attributeFieldTypeName = attributeFieldDefinition.FieldDefinition.Name;
            string name = partDefinition.Name + "." + attributeFieldDefinition.Name;
            switch(attributeFieldTypeName)
            {
                case nameof(BooleanProductAttributeField):
                    return new BooleanProductAttributeValue(name,
                        value != null && value.Contains("true", StringComparer.InvariantCultureIgnoreCase));
                case nameof(NumericProductAttributeField):
                    if (decimal.TryParse(value.FirstOrDefault(), out decimal decimalValue))
                    {
                        return new NumericProductAttributeValue(name, decimalValue);
                    }
                    return new NumericProductAttributeValue(name, null);
                case nameof(TextProductAttributeField):
                    // TODO: use settings to validate the value, and parse differently if multiple values are allowed.
                    return new TextProductAttributeValue(name, value);
                default:
                    return null;
            }
        }
    }
}
