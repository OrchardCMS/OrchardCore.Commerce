using OrchardCore.ContentManagement;
using OrchardCore.Media.Fields;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// The product part describes the most basic product attribute: a SKU. It also identifies any content item as a product,
/// by its mere presence.
/// </summary>
public class ProductPart : ContentPart
{
    /// <summary>
    /// Gets or sets the product's SKU, which can also be used as an alias for the item.
    /// </summary>
    public string Sku { get; set; }
    public IDictionary<string, bool> CanBeBought { get; } = new Dictionary<string, bool>();
    public MediaField ProductImage { get; set; }
}
