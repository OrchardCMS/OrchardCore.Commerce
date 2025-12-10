using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Inventory.Models;

public class InventoryPart : ContentPart
{
    public BooleanField AllowsBackOrder { get; set; } = new();
    public BooleanField IgnoreInventory { get; set; } = new();

    public IDictionary<string, int> Inventory { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public NumericField MaximumOrderQuantity { get; set; } = new();
    public NumericField MinimumOrderQuantity { get; set; } = new();
    public HtmlField OutOfStockMessage { get; set; } = new();

    public IList<string> InventoryKeys { get; } = [];

    public string ProductSku { get; set; }
}
