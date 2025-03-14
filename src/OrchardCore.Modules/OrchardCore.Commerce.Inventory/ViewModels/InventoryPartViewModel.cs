using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Inventory.ViewModels;

public class InventoryPartViewModel
{
    public bool AllowsBackOrder { get; set; }
    public bool IgnoreInventory { get; set; }

    public IDictionary<string, int> Inventory { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public int MaximumOrderQuantity { get; set; }
    public int MinimumOrderQuantity { get; set; }
    public string OutOfStockMessage { get; set; }
}
