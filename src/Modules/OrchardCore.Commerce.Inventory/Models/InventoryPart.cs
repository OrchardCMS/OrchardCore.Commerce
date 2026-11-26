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

    public void SetInventory(IList<string> keys, IList<int> inventory)
    {
        Inventory.Clear();
        InventoryKeys.SetItems(keys);

        for (var i = 0; i < keys.Count; i++)
        {
            Inventory.Add(keys[i], inventory[i]);
        }
    }

    public static string GetUpdatedInventoryKey(string oldKey, string oldSku, string newSku)
    {
        var updatedKey = newSku;
        var suffix = oldKey;
        if (suffix.StartsWithOrdinalIgnoreCase(oldSku + '-')) suffix = suffix[(oldSku.Length + 1)..];
        updatedKey += $"-{suffix}";

        return updatedKey;
    }
}
