using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
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
    public const string Text = nameof(Text);

    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IPredefinedValuesProductAttributeService _predefinedValuesProductAttributeService;
    private readonly IProductAttributeService _productAttributeService;

    public TextProductAttributeProvider(
        IContentDefinitionManager contentDefinitionManager,
        IPredefinedValuesProductAttributeService predefinedValuesProductAttributeService,
        IProductAttributeService productAttributeService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _predefinedValuesProductAttributeService = predefinedValuesProductAttributeService;
        _productAttributeService = productAttributeService;
    }

    public IProductAttributeValue Parse(
        ContentTypePartDefinition partDefinition,
        ContentPartFieldDefinition attributeFieldDefinition,
        string[] value) =>
            new TextProductAttributeValue(partDefinition.Name + "." + attributeFieldDefinition.Name, value);

    public void HandleSelectedAttributes(
        IDictionary<string, IDictionary<string, string>> selectedAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        var selectedTextAttributes = new Dictionary<string, string>();
        if (selectedAttributes.TryGetValue(Text, out var selectedTextAttributesRaw))
        {
            selectedTextAttributes = selectedTextAttributesRaw
                .Where(keyValuePair => keyValuePair.Value != null)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
        else
        {
            selectedAttributes.Add(Text, new Dictionary<string, string>());
        }

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
            var (attributePartDefinition, attributeFieldDefinition) = _productAttributeService.GetFieldDefinition(
                type, type.Name + "." + attribute.Name);

            var settings = (TextProductAttributeFieldSettings)attribute.Settings;
            var predefinedStrings = settings.PredefinedValues.Select(value => value.ToString());

            var value = predefinedStrings.First(item => item == selectedTextAttributes[attribute.Name]);

            if (Parse(attributePartDefinition, attributeFieldDefinition, new[] { value }) is { } matchingAttribute)
            {
                attributesList.Add(matchingAttribute);
            }
        }
    }

    public IDictionary<string, IDictionary<string, string>> GetSelectedAttributes(ISet<IProductAttributeValue> attributes) =>
        new Dictionary<string, IDictionary<string, string>>
        {
            {
                Text,
                attributes
                    .CastWhere<TextProductAttributeValue>()
                    .ToDictionary(attr => attr.FieldName, attr => attr.PredefinedValue)
            },
        };
}
