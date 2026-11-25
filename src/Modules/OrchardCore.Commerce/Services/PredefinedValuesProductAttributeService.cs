using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Mvc.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class PredefinedValuesProductAttributeService : IPredefinedValuesProductAttributeService
{
    private readonly IProductAttributeService _productAttributeService;

    public PredefinedValuesProductAttributeService(IProductAttributeService productAttributeService) =>
        _productAttributeService = productAttributeService;

    public async Task<IEnumerable<ProductAttributeDescription>> GetProductAttributesRestrictedToPredefinedValuesAsync(ContentItem product) =>
        (await _productAttributeService.GetProductAttributeFieldsAsync(product))
            .Where(description => description.Settings is
                IPredefinedValuesProductAttributeFieldSettings { RestrictToPredefinedValues: true })
            .OrderBy(description => description.PartName, StringComparer.Ordinal)
            .ThenBy(description => description.Name, StringComparer.Ordinal);

    public async Task<string> GetProductKey(ContentItem product, ICollection<IProductAttributeValue> attributes, IShoppingCartSerializer serializer)
    {
        var productPart = product.GetOrCreate<ProductPart>();
        var predefinedAttributeValues = (await GetProductAttributesRestrictedToPredefinedValuesAsync(product))
            .Select(attr => attr.PartName + "." + attr.Name)
            .ToHashSet();

        if (predefinedAttributeValues.Count == 0) return null;

        if (attributes.HasRawAttributes())
        {
            attributes.SetItems(await serializer.PostProcessAttributesAsync(attributes, productPart));
        }

        var predefinedAttributes = attributes
            .OfType<IPredefinedValuesProductAttributeValue>()
            .Where(attribute => predefinedAttributeValues.Contains(attribute.AttributeName))
            .OrderBy(value => value.AttributeName, StringComparer.Ordinal);

        return string
            .Join('-', predefinedAttributes.Select(attr => attr.UntypedPredefinedValue).Where(value => value != null))
            .HtmlClassify();
    }
}
