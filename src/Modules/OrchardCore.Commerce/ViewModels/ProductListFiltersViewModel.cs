using OrchardCore.Commerce.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class ProductListFiltersViewModel
{
    public ProductListPart ProductListPart { get; set; }
    public IEnumerable<string> OrderByOptions { get; set; }
    public IEnumerable<string> FilterIds { get; set; }
}
