using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Media.Fields;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// The product part describes the most basic product attribute: a SKU. It also identifies any content item as a product,
/// by its mere presence.
/// </summary>
public class ProductPart : ContentPart, ISkuHolderContent
{
    /// <inheritdoc cref="ISkuHolder.Sku"/>
    public string Sku { get; set; }

    /// <summary>
    /// Gets whether the product can currently be bought based on current inventory settings. If there is no
    /// <see cref="InventoryPart">InventoryPart</see> on the product, it is unused. This is not editable in the
    /// product's editor.
    /// </summary>
    public IDictionary<string, bool> CanBeBought { get; } = new Dictionary<string, bool>();

    /// <summary>
    /// Gets or sets the image associated with this product, which will be displayed on the product's page.
    /// </summary>
    public MediaField ProductImage { get; set; }
}
