using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class ProductListFilters
{
    public IList<string> OrderBy { get; } = [];
    public IDictionary<string, string> FilterValues { get; } = new Dictionary<string, string>();
}
