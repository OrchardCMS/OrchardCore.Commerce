using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPredefinedValuesProductAttributeService
    {
        IEnumerable<ProductAttributeDescription> GetProductAttributesRestrictedToPredefinedValues(ContentItem product);
        IEnumerable<IEnumerable<object>> GetProductAttributesPredefinedValues(ContentItem product);
        IEnumerable<string> GetProductAttributesCombinations(ContentItem product);
    }
}
