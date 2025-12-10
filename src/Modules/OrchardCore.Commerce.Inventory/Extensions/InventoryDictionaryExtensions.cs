using OrchardCore.Commerce.Inventory.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Inventory;

public static class InventoryDictionaryExtensions
{
    public static IDictionary<string, int> FilterOutdatedEntries(
        this InventoryPart part) =>
            part
                .Inventory
                .Where(inventory => part.InventoryKeys.Count == 0 || part.InventoryKeys.Contains(inventory.Key))
                .ToDictionary(key => key.Key, value => value.Value);
}
