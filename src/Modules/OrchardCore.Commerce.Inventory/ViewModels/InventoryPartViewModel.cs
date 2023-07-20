using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Inventory.ViewModels;

public class InventoryPartViewModel
{
    public bool AllowsBackOrder { get; set; }
    public bool IgnoreInventory { get; set; }

    public IDictionary<string, int> InventoryValues { get; private set; } = new Dictionary<string, int>();

    [BindNever]
    public IDictionary<string, int> Inventory { get; private set; } = new Dictionary<string, int>();

    public int MaximumOrderQuantity { get; set; }
    public int MinimumOrderQuantity { get; set; }
    public string OutOfStockMessage { get; set; }
    public string ProductSku { get; set; }

    public void InitializeInventory(IDictionary<string, int> inventory, IDictionary<string, int> inventoryValues)
    {
        Inventory = inventory;
        InventoryValues = inventoryValues;
    }
}
