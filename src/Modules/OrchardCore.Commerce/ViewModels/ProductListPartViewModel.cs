using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class ProductListPartViewModel
{
    public ProductListPart ProductListPart { get; set; }
    public IShape Pager { get; set; }
    public IEnumerable<ContentItem> Products { get; set; }
    public BuildPartDisplayContext Context { get; set; }
}
