using OrchardCore.Navigation;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class ProductListFilterParameters
{
    public Pager Pager { get; set; }
    public string OrderBy { get; set; }
    public IDictionary<string, string> FilterValues { get; } = new Dictionary<string, string>();
}
