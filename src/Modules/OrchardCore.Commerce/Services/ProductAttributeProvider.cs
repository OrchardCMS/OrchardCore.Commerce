using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.ContentManagement.Metadata.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace OrchardCore.Commerce.Services;

public class ProductAttributeProvider : IProductAttributeProvider
{
    private static readonly string[] _supportedTypeNames =
    {
        nameof(BooleanProductAttributeField),
        nameof(NumericProductAttributeField),
        nameof(TextProductAttributeField),
    };

    public IProductAttributeValue CreateFromValue(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        object value)
    {
        var attributeFieldTypeName = attributeFieldDefinition.FieldDefinition.Name;
        if (value == null || !_supportedTypeNames.Contains(attributeFieldTypeName)) return null;

        var name = partDefinition.Name + "." + attributeFieldDefinition.Name;
        var element = value switch
        {
            JsonElement jsonElement => jsonElement,
            JToken jToken => JsonDocument.Parse(JsonConvert.SerializeObject(jToken)).RootElement,
            _ => default,
        };

        return attributeFieldTypeName switch
        {
            nameof(BooleanProductAttributeField) => new BooleanProductAttributeValue(name, element.GetBoolean()),
            nameof(NumericProductAttributeField) => element.TryGetDecimal(out var decimalValue)
                ? new NumericProductAttributeValue(name, decimalValue)
                : new NumericProductAttributeValue(name, value: null),
            nameof(TextProductAttributeField) => element.ValueKind switch
            {
                JsonValueKind.String => new TextProductAttributeValue(name, element.GetString()),
                JsonValueKind.Array => new TextProductAttributeValue(
                    name,
                    element.EnumerateArray().Select(el => el.GetString())),
                _ => new TextProductAttributeValue(name, values: (IEnumerable<string>)null), // The cast prevents S3220.
            },
            _ => null,
        };
    }

    public IDictionary<string, IDictionary<string, string>> GetSelectedAttributes(ISet<IProductAttributeValue> attributes) =>
        new Dictionary<string, IDictionary<string, string>>();

    public void HandleSelectedAttributes(
        IDictionary<string, IDictionary<string, string>> selectedAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        return;
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
                    value?.Contains("true", StringComparer.InvariantCultureIgnoreCase) == true);
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
