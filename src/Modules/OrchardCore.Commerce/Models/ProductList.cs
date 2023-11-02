using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

public class ProductList
{
    public IEnumerable<ContentItem> Products { get; set; }
    public int TotalItemCount { get; set; }
}
