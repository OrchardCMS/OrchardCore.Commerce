using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IProductAttributeService
    {
        IEnumerable<ProductAttributeDescription> GetProductAttributeFields(ContentItem product);
    }
}
