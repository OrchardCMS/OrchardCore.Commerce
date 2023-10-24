using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OrchardCore.Commerce.Services;

public class NumericProductAttributeProvider : IProductAttributeProvider
{
    private readonly IProductAttributeService _productAttributeService;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public NumericProductAttributeProvider(
        IProductAttributeService productAttributeService,
        IContentDefinitionManager contentDefinitionManager)
    {
        _productAttributeService = productAttributeService;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public IProductAttributeValue Parse(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        string[] value)
    {
        var name = partDefinition.Name + "." + attributeFieldDefinition.Name;

        if (decimal.TryParse(value.FirstOrDefault(), out var decimalValue))
        {
            return new NumericProductAttributeValue(name, decimalValue);
        }

        return new NumericProductAttributeValue(name, value: null);
    }

    public void HandleSelectedAttributes(
        IDictionary<string, IDictionary<string, string>> selectedAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        var selectedNumericAttributes = new Dictionary<string, string>();
        if (selectedAttributes.TryGetValue("Numeric", out var selectedNumericAttributesRaw))
        {
            selectedNumericAttributes = selectedNumericAttributesRaw.ToDictionary(
                pair => pair.Key, pair => pair.Value);
        }
        else
        {
            selectedAttributes.Add("Numeric", new Dictionary<string, string>());
        }

        var numericAttributesList = _productAttributeService.GetProductAttributeFields(productPart.ContentItem)
            .Where(attr => attr.Field is NumericProductAttributeField)
            .ToList();

        // Construct actual attributes from strings.
        var type = _contentDefinitionManager.GetTypeDefinition(productPart.ContentItem.ContentType);
        foreach (var attribute in numericAttributesList)
        {
            var (attributePartDefinition, attributeFieldDefinition) = _productAttributeService.GetFieldDefinition(
                type, type.Name + "." + attribute.Name);

            var defaultValue = ((attribute.Settings as NumericProductAttributeFieldSettings).DefaultValue ?? 0)
                .ToString(CultureInfo.InvariantCulture);
            var selectedNumericAttribute = selectedNumericAttributes.FirstOrDefault(keyValuePair => keyValuePair.Key == attribute.Name);
            var selectedValue = selectedNumericAttribute.Value;

            var matchingAttribute = Parse(
                    attributePartDefinition,
                    attributeFieldDefinition,
                    new[] { selectedValue ?? defaultValue });

            attributesList.Add(matchingAttribute);
        }
    }
}
