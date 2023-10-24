using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Services;

public class BooleanProductAttributeProvider : IProductAttributeProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IProductAttributeService _productAttributeService;

    public BooleanProductAttributeProvider(
        IContentDefinitionManager contentDefinitionManager,
        IProductAttributeService productAttributeService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _productAttributeService = productAttributeService;
    }

    public IProductAttributeValue Parse(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        string[] value) =>
            new BooleanProductAttributeValue(
                partDefinition.Name + "." + attributeFieldDefinition.Name,
                value?.Contains("true", StringComparer.InvariantCultureIgnoreCase) == true);

    public void HandleSelectedAttributes(
        IDictionary<string, IDictionary<string, string>> selectedAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        var selectedBooleanAttributes = new Dictionary<string, string>();
        if (selectedAttributes.TryGetValue("Boolean", out var selectedBooleanAttributesRaw))
        {
            selectedBooleanAttributes = selectedBooleanAttributesRaw.ToDictionary(
                pair => pair.Key, pair => pair.Value);
        }
        else
        {
            selectedAttributes.Add("Boolean", new Dictionary<string, string>());
        }

        var booleanAttributesList = _productAttributeService.GetProductAttributeFields(productPart.ContentItem)
            .Where(attr => attr.Field is BooleanProductAttributeField)
            .Select(attr => attr.Name)
            .ToList();

        // Construct actual attributes from strings.
        var type = _contentDefinitionManager.GetTypeDefinition(productPart.ContentItem.ContentType);
        foreach (var attribute in booleanAttributesList)
        {
            var (attributePartDefinition, attributeFieldDefinition) = _productAttributeService.GetFieldDefinition(
                type, type.Name + "." + attribute);

            // The value is true if the selected boolean attributes list contains the attribute, otherwise false.
            var value = selectedBooleanAttributes.Any(keyValuePair => keyValuePair.Key == attribute);
            var matchingAttribute = Parse(attributePartDefinition, attributeFieldDefinition, new[] { value.ToString() });

            attributesList.Add(matchingAttribute);
        }
    }
}
