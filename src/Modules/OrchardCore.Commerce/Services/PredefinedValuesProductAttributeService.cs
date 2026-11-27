using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

[Obsolete($"Use {nameof(ProductAttributeService)} instead.")]
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
}
