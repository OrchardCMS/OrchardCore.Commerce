using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPriceVariantsService
    {
        Dictionary<string, decimal> GetPriceVariants(ContentItem product);
    }
}
