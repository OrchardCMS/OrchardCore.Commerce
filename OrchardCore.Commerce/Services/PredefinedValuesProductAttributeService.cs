using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Services;

public class PredefinedValuesProductAttributeService : IPredefinedValuesProductAttributeService
{
    private readonly IProductAttributeService _productAttributeService;

    public PredefinedValuesProductAttributeService(IProductAttributeService productAttributeService) =>
        _productAttributeService = productAttributeService;

    public IEnumerable<ProductAttributeDescription> GetProductAttributesRestrictedToPredefinedValues(ContentItem product)
        => _productAttributeService
            .GetProductAttributeFields(product)
            .Where(x => x.Settings is IPredefinedValuesProductAttributeFieldSettings { RestrictToPredefinedValues: true })
            .OrderBy(x => x.PartName)
            .ThenBy(x => x.Name);
}
