using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Commerce.Models;

public class ProductListFilterContext
{
    public ProductListPart ProductList { get; set; }
    public ProductListFilterParameters FilterParameters { get; set; }
    public IQuery<ContentItem> Query { get; set; }
}
