using OrchardCore.ContentManagement;
using OrchardCore.Navigation;
using System.Collections.Generic;
using YesSql;

namespace OrchardCore.Commerce.Models;

public class ProductListQueryContext
{
    public ProductListPart ProductList { get; set; }
    public Pager Pager { get; set; }
    public IQuery<ContentItem> Query { get; set; }
    public IEnumerable<ContentItem> QueriedProducts { get; set; }
}
