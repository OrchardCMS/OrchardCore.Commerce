using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class ProductListFilters
{
    public IList<string> OrderBy { get; } = new List<string>();
    public IDictionary<string, string> FilterValues { get; } = new Dictionary<string, string>();
}
