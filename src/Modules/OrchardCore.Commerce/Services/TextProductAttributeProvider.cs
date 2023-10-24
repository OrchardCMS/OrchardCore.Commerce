using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Services;

public class TextProductAttributeProvider : IProductAttributeProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IPredefinedValuesProductAttributeService _predefinedValuesProductAttributeService;

    public TextProductAttributeProvider(
        IContentDefinitionManager contentDefinitionManager,
        IPredefinedValuesProductAttributeService predefinedValuesProductAttributeService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _predefinedValuesProductAttributeService = predefinedValuesProductAttributeService;
    }

    public IProductAttributeValue Parse(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        string[] value) =>
            new TextProductAttributeValue(partDefinition.Name + "." + attributeFieldDefinition.Name, value);

    //public IDictionary<string, string> GetSelectedAttributes()
    //{

    //}

    public void HandleSelectedAttributes(
        IDictionary<string, IDictionary<string, string>> selectedAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        var selectedTextAttributes = new Dictionary<string, string>();
        if (selectedAttributes.TryGetValue("Text", out var selectedTextAttributesRaw))
        {
            selectedTextAttributes = selectedTextAttributesRaw
                //.Where(keyValuePair => keyValuePair.Value != null) // what if we just go without this
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        //else
        //{
        //    selectedAttributes["Text"].Add();
        //}

        // if there are no 

        var predefinedAttributes = _predefinedValuesProductAttributeService
            .GetProductAttributesRestrictedToPredefinedValues(productPart.ContentItem);

        // Predefined attributes must contain the selected attributes.
        var selectedTextAttributesList = predefinedAttributes
            .Where(predefinedAttr => selectedTextAttributes.Any(selectedAttr => selectedAttr.Key.Contains(predefinedAttr.Name)))
            .ToList();

        // Construct actual attributes from strings.
        var type = _contentDefinitionManager.GetTypeDefinition(productPart.ContentItem.ContentType);
        foreach (var attribute in selectedTextAttributesList)
        {
            var (attributePartDefinition, attributeFieldDefinition) = GetFieldDefinition(
                type, type.Name + "." + attribute.Name);

            var predefinedStrings = new List<string>();
            predefinedStrings.AddRange(
                (attribute.Settings as TextProductAttributeFieldSettings).PredefinedValues.Select(value => value.ToString()));

            var value = predefinedStrings.First(
                item => item == selectedTextAttributes.First(keyValuePair => keyValuePair.Key == attribute.Name).Value);

            var matchingAttribute = Parse(attributePartDefinition, attributeFieldDefinition, new[] { value });

            // add here only if not null?
            attributesList.Add(matchingAttribute);
        }
    }

    private static (ContentTypePartDefinition PartDefinition, ContentPartFieldDefinition FieldDefinition)
        GetFieldDefinition(ContentTypeDefinition type, string attributeName)
    {
        var partAndField = attributeName.Split('.');
        var partName = partAndField[0];
        var fieldName = partAndField[1];

        return type
            .Parts
            .Where(partDefinition => partDefinition.Name == partName)
            .SelectMany(partDefinition => partDefinition
                .PartDefinition
                .Fields
                .Select(fieldDefinition => (PartDefinition: partDefinition, FieldDefinition: fieldDefinition))
                .Where(pair => pair.FieldDefinition.Name == fieldName))
            .FirstOrDefault();
    }
}
