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

    public void ResetInventoryKeys() => InventoryKeys.SetItems(Inventory.Keys);

    public static string GetUpdatedInventoryKey(string oldKey, string oldSku, string newSku)
    {
        var updatedKey = newSku;
        var suffix = oldKey;
        if (suffix.StartsWithOrdinalIgnoreCase(oldSku + '-')) suffix = suffix[(oldSku.Length + 1)..];
        updatedKey += $"-{suffix}";

        return updatedKey;
    }

    public static string GetInventoryDisplay(
        IDictionary<string, int> inventory,
        string key,
        string sku)
    {
        if (inventory.Count < 2 || string.IsNullOrWhiteSpace(key) || !inventory.ContainsKey(key))
        {
            return string.Empty;
        }

        return key.StartsWithOrdinalIgnoreCase(sku + '-') ? key[(sku.Length + 1)..] : key;
    }
}
