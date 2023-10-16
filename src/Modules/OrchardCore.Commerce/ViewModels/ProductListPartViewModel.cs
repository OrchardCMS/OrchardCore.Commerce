using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.Models;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class ProductListPartViewModel
{
    public ProductListPart ProductListPart { get; set; }
    public dynamic Pager { get; set; }
    public IEnumerable<ContentItem> Products { get; set; }
    public BuildPartDisplayContext Context { get; set; }
}
