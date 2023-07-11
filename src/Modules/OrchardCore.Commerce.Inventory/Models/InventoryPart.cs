using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using System;

namespace OrchardCore.Commerce.Inventory.Models;

public class InventoryPart : ContentPart
{
    public BooleanField AllowsBackOrder { get; set; } = new();
    public BooleanField IgnoreInventory { get; set; } = new();
    public NumericField Inventory { get; set; } = new(); // become dictionary of string and int
    public IDictionary<string, int> Inventoree { get; } = new Dictionary<string, int>();

    public NumericField MaximumOrderQuantity { get; set; } = new(); // u too
    public NumericField MinimumOrderQuantity { get; set; } = new(); // same
    public HtmlField OutOfStockMessage { get; set; } = new();
}
