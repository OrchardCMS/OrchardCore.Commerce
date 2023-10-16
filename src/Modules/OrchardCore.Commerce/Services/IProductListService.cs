using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Navigation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public interface IProductListService
{
    Task<IEnumerable<ContentItem>> GetProductsAsync(ProductListPart productList, Pager pager);
}