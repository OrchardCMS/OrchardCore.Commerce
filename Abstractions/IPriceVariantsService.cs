using System.Collections.Generic;
using System.Threading.Tasks;
using Money;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPriceVariantsService
    {
        Dictionary<string, Amount> GetPriceVariants(ContentItem product);
    }
}
