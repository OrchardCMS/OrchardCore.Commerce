using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Inventory.Local.Models;

public class InventoryPart : ContentPart
{
    public BooleanField AllowsBackOrder { get; set; } = new();
    public BooleanField IgnoreInventory { get; set; } = new();
    public NumericField Inventory { get; set; } = new();
    public NumericField MaximumOrderQuantity { get; set; } = new();
    public NumericField MinimumOrderQuantity { get; set; } = new();
    public HtmlField OutOfStockMessage { get; set; } = new();
}
