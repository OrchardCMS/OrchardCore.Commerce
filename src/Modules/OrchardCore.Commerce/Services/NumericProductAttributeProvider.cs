using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ProductAttributeValues;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class NumericProductAttributeProvider : IProductAttributeProvider
{
    public const string Numeric = nameof(Numeric);

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

        return decimal.TryParse(value.FirstOrDefault(), out var decimalValue)
            ? new NumericProductAttributeValue(name, decimalValue)
            : new NumericProductAttributeValue(name, value: null);
    }

    public async Task HandleSelectedAttributesAsync(
        IDictionary<string, IDictionary<string, string>> selectedAttributes,
        ProductPart productPart,
        IList<IProductAttributeValue> attributesList)
    {
        var selectedNumericAttributes = new Dictionary<string, string>();
        if (selectedAttributes.TryGetValue(Numeric, out var selectedNumericAttributesRaw))
        {
            selectedNumericAttributes = selectedNumericAttributesRaw.ToDictionary(
                pair => pair.Key, pair => pair.Value);
        }
        else
        {
            selectedAttributes.Add(Numeric, new Dictionary<string, string>());
        }

        var numericAttributesList = (await _productAttributeService.GetProductAttributeFieldsAsync(productPart.ContentItem))
            .Where(attr => attr.Field is NumericProductAttributeField)
            .ToList();

        // Construct actual attributes from strings.
        var type = await _contentDefinitionManager.GetTypeDefinitionAsync(productPart.ContentItem.ContentType);
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
                    [selectedValue ?? defaultValue]);

            if (matchingAttribute is not null) attributesList.Add(matchingAttribute);
        }
    }

    public IDictionary<string, IDictionary<string, string>> GetSelectedAttributes(ISet<IProductAttributeValue> attributes) =>
        new Dictionary<string, IDictionary<string, string>>
        {
            {
                Numeric,
                attributes
                    .CastWhere<NumericProductAttributeValue>()
                    .ToDictionary(attr => attr.FieldName, attr => attr.UntypedValue?.ToString())
            },
        };
}
