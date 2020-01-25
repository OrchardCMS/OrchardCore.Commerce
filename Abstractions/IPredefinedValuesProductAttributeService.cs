using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPredefinedValuesProductAttributeService
    {
        IEnumerable<IEnumerable<object>> GetProductAttributesPredefinedValues(ContentItem product);
        IEnumerable<string> GetProductAttributesCombinations(ContentItem product);
    }
}
