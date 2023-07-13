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

    public async Task<IDictionary<string, int>> QueryAllInventoriesAsync(string sku)
    {
        var inventoryPart = (await _productService.GetProductAsync(sku))?.As<InventoryPart>();
        return inventoryPart?.Inventoree;
    }

    public async Task<int> QueryInventoryAsync(string sku, string fullSku = null)
    {
        var inventoryPart = (await _productService.GetProductAsync(sku))?.As<InventoryPart>();

        // If fullSku is specified, look for Price Variant Product's inventory.
        var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? "DEFAULT" : fullSku.ToUpperInvariant();
        var relevantInventory = inventoryPart?.Inventoree.FirstOrDefault(entry => entry.Key == inventoryIdentifier);

        return relevantInventory is { } inventory ? inventory.Value : 0;
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

    private async Task UpdateInventoryAsync(ProductPart productPart, int difference, string fullSku = null)
    {
        await _lock.WaitAsync();

        try
        {
            var inventoryPart = productPart.As<InventoryPart>();
            if (inventoryPart == null || inventoryPart.IgnoreInventory.Value) return;

            var inventoryIdentifier = string.IsNullOrEmpty(fullSku) ? "DEFAULT" : fullSku.ToUpperInvariant();
            var relevantInventory = inventoryPart?.Inventoree.FirstOrDefault(entry => entry.Key == inventoryIdentifier);

            var newValue = relevantInventory.Value.Value + difference;
            if (newValue < 0)
            {
                throw new InvalidOperationException("Inventory value cannot be negative.");
            }

            var newEntry = new KeyValuePair<string, int>(relevantInventory.Value.Key, newValue);

            inventoryPart.Inventoree.Remove(inventoryIdentifier);
            inventoryPart.Inventoree.Add(newEntry);
            inventoryPart.Apply();

            _session.Save(productPart.ContentItem);
        }
        finally
        {
            _lock.Release();
        }
    }
}
