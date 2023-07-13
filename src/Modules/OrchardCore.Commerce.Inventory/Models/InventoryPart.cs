using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Inventory.Models;

public class InventoryPart : ContentPart
{
    public BooleanField AllowsBackOrder { get; set; } = new();
    public BooleanField IgnoreInventory { get; set; } = new();
    public IDictionary<string, int> Inventory { get; } = new Dictionary<string, int>();

    public NumericField MaximumOrderQuantity { get; set; } = new(); // u too
    public NumericField MinimumOrderQuantity { get; set; } = new(); // same
    public HtmlField OutOfStockMessage { get; set; } = new();
}
