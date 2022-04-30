using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.ContentManagement.Metadata.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace OrchardCore.Commerce.Services;

public class ProductAttributeProvider : IProductAttributeProvider
{
    public IProductAttributeValue CreateFromJsonElement(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        JsonElement value)
    {
        var attributeFieldTypeName = attributeFieldDefinition.FieldDefinition.Name;
        var name = partDefinition.Name + "." + attributeFieldDefinition.Name;
        return attributeFieldTypeName switch
        {
            nameof(BooleanProductAttributeField) => new BooleanProductAttributeValue(name, value.GetBoolean()),
            nameof(NumericProductAttributeField) => value.TryGetDecimal(out var decimalValue)
                ? new NumericProductAttributeValue(name, decimalValue)
                : new NumericProductAttributeValue(name, value: null),
            nameof(TextProductAttributeField) => value.ValueKind switch
            {
                JsonValueKind.String => new TextProductAttributeValue(name, value.GetString()),
                JsonValueKind.Array => new TextProductAttributeValue(
                    name,
                    value.EnumerateArray().Select(el => el.GetString())),
                _ => new TextProductAttributeValue(name, values: (IEnumerable<string>)null), // The cast prevents S3220.
            },
            _ => null,
        };
    }

    public IProductAttributeValue Parse(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        string[] value)
    {
        var attributeFieldTypeName = attributeFieldDefinition.FieldDefinition.Name;
        var name = partDefinition.Name + "." + attributeFieldDefinition.Name;
        switch (attributeFieldTypeName)
        {
            case nameof(BooleanProductAttributeField):
                return new BooleanProductAttributeValue(
                    name,
                    value != null && value.Contains("true", StringComparer.InvariantCultureIgnoreCase));
            case nameof(NumericProductAttributeField):
                if (decimal.TryParse(value.FirstOrDefault(), out var decimalValue))
                {
                    return new NumericProductAttributeValue(name, decimalValue);
                }

                return new NumericProductAttributeValue(name, value: null);
            case nameof(TextProductAttributeField):
                return new TextProductAttributeValue(name, value);
            default:
                return null;
        }
    }
}
