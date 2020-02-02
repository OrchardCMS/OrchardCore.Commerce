using System.Collections.Generic;
using Money;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPriceVariantsService
    {
        Dictionary<string, Amount> GetPriceVariants(ContentItem product);
    }
}
