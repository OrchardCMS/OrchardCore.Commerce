using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Commerce.Services;

/// <summary>
/// A product inventory provider that updates inventories of shopping cart items which have an <c>InventoryPart</c>.
/// </summary>
public class LocalInventoryProvider : IProductInventoryProvider
{
    private readonly IProductService _productService;
    private readonly ISession _session;
    private readonly IPredefinedValuesProductAttributeService _predefinedValuesService;
    private static readonly SemaphoreSlim _lock = new(initialCount: 1);

    public LocalInventoryProvider(
        IProductService productService,
        ISession session,
        IPredefinedValuesProductAttributeService predefinedValuesService)
    {
        _productService = productService;
        _session = session;
        _predefinedValuesService = predefinedValuesService;
    }

    public int Order => 0;

    public Task<bool> IsApplicableAsync(IList<ShoppingCartItem> model) => Task.FromResult(true);

    // maybe make new method QueryAllInventoriesAsync() to query all inventories

    // needs to return dictionary? Or should it only query one specific inventory instead of all?
        // if this remains, it should be QueryProductInventory
    public async Task<int> QueryInventoryAsync(string sku)
    {
        var inventoryPart = (await _productService.GetProductAsync(sku))?.As<InventoryPart>();

        var inventoree = inventoryPart?.Inventoree.FirstOrDefault();

        return inventoryPart?.Inventory.Value is { } inventory ? (int)inventory : 0;
    }

    public async Task<IList<ShoppingCartItem>> UpdateAsync(IList<ShoppingCartItem> model)
    {
        foreach (var item in model)
        {
            var productPart = await _productService.GetProductAsync(item.ProductSku);
            var attributesRestrictedToPredefinedValues = _predefinedValuesService
                .GetProductAttributesRestrictedToPredefinedValues(productPart.ContentItem)
                .Select(attr => attr.PartName + "." + attr.Name)
                .ToHashSet();

            var variantKey = item.GetVariantKeyFromAttributes(attributesRestrictedToPredefinedValues);

            var fullSku = item.Attributes.Any()
                ? item.ProductSku + "-" + variantKey
                : string.Empty;

            await UpdateInventoryAsync(
                await _productService.GetProductAsync(item.ProductSku),
                -item.Quantity,
                fullSku);
        }

        return model;
    }

    private async Task UpdateInventoryAsync(ProductPart productPart, int difference, string fullSku = null) // also needs dictionary key?
    {
        await _lock.WaitAsync();

        try
        {
            var inventoryPart = productPart.As<InventoryPart>(); // this contains inventory of both regular products and pricevariant products
            if (inventoryPart == null || inventoryPart.IgnoreInventory.Value) return;

            var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? "Default" : fullSku;

            var relevantInventory = inventoryPart.Inventoree.FirstOrDefault(entry => entry.Key == inventoryIdentifier);
            var newValue = relevantInventory.Value + difference; // rename
            if (newValue < 0)
            {
                throw new InvalidOperationException("Inventory value cannot be negative.");
            }

            var newEntry = new KeyValuePair<string, int>(relevantInventory.Key, newValue);

            inventoryPart.Inventoree.Remove(inventoryIdentifier);
            inventoryPart.Inventoree.Add(newEntry);


            // old
            var oldnewValue = ((int)inventoryPart.Inventory.Value) + difference;
            if (oldnewValue < 0)
            {
                throw new InvalidOperationException("Inventory value cannot be negative.");
            }

            inventoryPart.Inventory.Value = oldnewValue;
            // /old


            inventoryPart.Apply();

            _session.Save(productPart.ContentItem);
        }
        finally
        {
            _lock.Release();
        }
    }
}
