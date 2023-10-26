using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Display.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class ProductListOrderByViewModel
{
    public ProductListPart ProductListPart { get; set; }
    public IEnumerable<string> OrderByOptions { get; set; }
}
