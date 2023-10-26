using OrchardCore.ContentManagement;
using OrchardCore.Navigation;
using System.Collections.Generic;
using YesSql;

namespace OrchardCore.Commerce.Models;

public class ProductListFilterContext
{
    public ProductListPart ProductList { get; set; }
    public ProductListFilterParameters FilterParameters { get; set; }
    public IQuery<ContentItem> Query { get; set; }
}

public class ProductListFilterParameters
{
    public Pager Pager { get; set; }
    public IList<string> OrderBy { get; } = new List<string>();
    public IDictionary<string, string> FilterValues { get; } = new Dictionary<string, string>();
}
