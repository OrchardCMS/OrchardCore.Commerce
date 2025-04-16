using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.ContentManagement;
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
            .OrderBy(description => description.PartName)
            .ThenBy(description => description.Name);
}
