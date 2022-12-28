using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
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

    public async Task<int> QueryInventoryAsync(string sku)
    {
        var inventoryPart = (await _productService.GetProductAsync(sku))?.As<InventoryPart>();
        return inventoryPart?.Inventory.Value is { } inventory ? (int)inventory : 0;
    }

    public async Task<IList<ShoppingCartItem>> UpdateAsync(IList<ShoppingCartItem> model)
    {
        foreach (var item in model)
        {
            await UpdateInventoryAsync(await _productService.GetProductAsync(item.ProductSku), -item.Quantity);
        }

        return model;
    }

    private async Task UpdateInventoryAsync(ProductPart productPart, int difference)
    {
        await _lock.WaitAsync();

        try
        {
            var inventoryPart = productPart.As<InventoryPart>();
            if (inventoryPart == null || inventoryPart.IgnoreInventory.Value) return;

            var newValue = ((int)inventoryPart.Inventory.Value) + difference;
            if (newValue < 0)
            {
                throw new InvalidOperationException("Inventory value cannot be negative.");
            }

            inventoryPart.Inventory.Value = newValue;
            inventoryPart.Apply();

            _session.Save(productPart.ContentItem);
        }
        finally
        {
            _lock.Release();
        }
    }
}
