using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Inventory;

public static class InventoryDictionaryExtensions
{
    public static IDictionary<string, int> FilterOutdatedEntries(
        this IDictionary<string, int> inventory, IList<string> inventoryKeys) =>
            inventory
                .Where(inventory => inventoryKeys.Contains(inventory.Key))
                .ToDictionary(key => key.Key, value => value.Value);
}
