using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
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
    private static readonly SemaphoreSlim _lock = new(initialCount: 1);

    public LocalInventoryProvider(IProductService productService, ISession session)
    {
        _productService = productService;
        _session = session;
    }

    public int Order => 0;

    public Task<bool> IsApplicableAsync(IList<ShoppingCartItem> model) => Task.FromResult(true);

    public async Task<IDictionary<string, int>> QueryAllInventoriesAsync(string sku)
    {
        var inventoryPart = (await _productService.GetProductAsync(sku))?.As<InventoryPart>();
        return inventoryPart?.Inventory;
    }

    public async Task<int> QueryInventoryAsync(string sku, string fullSku = null)
    {
        var inventoryPart = (await _productService.GetProductAsync(sku))?.As<InventoryPart>();

        // If fullSku is specified, look for Price Variant Product's inventory.
        var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? sku : fullSku;
        var relevantInventory = inventoryPart?.Inventory.FirstOrDefault(entry => entry.Key == inventoryIdentifier);

        return relevantInventory is { } inventory ? inventory.Value : 0;
    }

    public async Task<IList<ShoppingCartItem>> UpdateAsync(IList<ShoppingCartItem> model)
    {
        foreach (var item in model)
        {
            var productPart = await _productService.GetProductAsync(item.ProductSku);
            var fullSku = _productService.GetOrderFullSku(item, productPart);

            await UpdateInventoryAsync(
                await _productService.GetProductAsync(item.ProductSku),
                -item.Quantity,
                fullSku);
        }

        return model;
    }

    private async Task UpdateInventoryAsync(ProductPart productPart, int difference, string fullSku = null)
    {
        await _lock.WaitAsync();

        try
        {
            var inventoryPart = productPart.ContentItem.As<InventoryPart>();
            if (inventoryPart == null || inventoryPart.IgnoreInventory.Value) return;

            var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? productPart.Sku : fullSku;
            var inventoryRootIdentifier = inventoryIdentifier.Contains('-')
                ? inventoryIdentifier.Split('-')[0]
                : inventoryIdentifier;

            var relevantInventory = inventoryPart.Inventory.FirstOrDefault(entry =>
                entry.Key == inventoryIdentifier || entry.Key == inventoryRootIdentifier);

            var newValue = relevantInventory.Value + difference < 0 && inventoryPart.AllowsBackOrder.Value
                ? 0
                : relevantInventory.Value + difference;

            if (newValue < 0)
            {
                throw new InvalidOperationException("Inventory value cannot be negative.");
            }

            var newEntry = new KeyValuePair<string, int>(relevantInventory.Key, newValue);

            inventoryPart.Inventory.Remove(relevantInventory.Key);
            inventoryPart.Inventory.Add(newEntry);
            inventoryPart.Apply();

            _session.Save(productPart.ContentItem);
        }
        finally
        {
            _lock.Release();
        }
    }
}
