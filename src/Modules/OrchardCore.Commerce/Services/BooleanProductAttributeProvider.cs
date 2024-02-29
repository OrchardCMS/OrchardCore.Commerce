using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class BooleanProductAttributeProvider : IProductAttributeProvider
{
    public const string Boolean = nameof(Boolean);

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

    public async Task HandleSelectedAttributesAsync(
        IDictionary<string, IDictionary<string, string>> selectedAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        var selectedBooleanAttributes = new Dictionary<string, string>();
        if (selectedAttributes.TryGetValue(Boolean, out var selectedBooleanAttributesRaw))
        {
            selectedBooleanAttributes = selectedBooleanAttributesRaw.ToDictionary(
                pair => pair.Key, pair => pair.Value);
        }
        else
        {
            selectedAttributes.Add(Boolean, new Dictionary<string, string>());
        }

        var booleanAttributesList = (await _productAttributeService.GetProductAttributeFieldsAsync(productPart.ContentItem))
            .Where(attr => attr.Field is BooleanProductAttributeField)
            .Select(attr => attr.Name)
            .ToList();

        // Construct actual attributes from strings.
        var type = await _contentDefinitionManager.GetTypeDefinitionAsync(productPart.ContentItem.ContentType);
        foreach (var attribute in booleanAttributesList)
        {
            var (attributePartDefinition, attributeFieldDefinition) = _productAttributeService.GetFieldDefinition(
                type, type.Name + "." + attribute);

            // The value is true if the selected boolean attributes list contains the attribute, otherwise false.
            var value = selectedBooleanAttributes.Any(keyValuePair => keyValuePair.Key == attribute);

            if (Parse(attributePartDefinition, attributeFieldDefinition, [value.ToString()]) is { } matchingAttribute)
            {
                attributesList.Add(matchingAttribute);
            }
        }
    }

    public IDictionary<string, IDictionary<string, string>> GetSelectedAttributes(ISet<IProductAttributeValue> attributes) =>
        new Dictionary<string, IDictionary<string, string>>
        {
            {
                Boolean,
                attributes
                    .CastWhere<BooleanProductAttributeValue>()
                    .ToDictionary(attr => attr.FieldName, attr => attr.UntypedValue?.ToString())
            },
        };
}
