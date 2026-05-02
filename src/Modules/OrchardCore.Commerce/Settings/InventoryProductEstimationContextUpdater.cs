using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Inventory.Models;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Settings;

public class InventoryProductEstimationContextUpdater : IProductEstimationContextUpdater
{
    private readonly IProductService _productService;
    private readonly IShoppingCartPersistence _shoppingCartPersistence;
    public int Order => 0;

    public InventoryProductEstimationContextUpdater(
        IProductService productService,
        IShoppingCartPersistence shoppingCartPersistence)
    {
        _productService = productService;
        _shoppingCartPersistence = shoppingCartPersistence;
    }

    public async Task<ProductEstimationContext> UpdateAsync(ProductEstimationContext model)
    {
        // If the product doesn't have InventoryPart then this event is not applicable.
        if (await _productService.GetProductAsync(model.ShoppingCartItem.ProductSku) is not { } product ||
            product.ContentItem.As<InventoryPart>() is not { } inventory)
        {
            return model;
        }

        var cart = await _shoppingCartPersistence.RetrieveAsync(model.ShoppingCartId);
        var item = cart.AddItem(model.ShoppingCartItem.WithQuantity(0));
        var newQuantity = item.Quantity + model.ShoppingCartItem.Quantity;

        var minimum = inventory.MinimumOrderQuantity.Value is { } minimumDecimal ? (int)minimumDecimal : int.MinValue;
        if (newQuantity < minimum)
        {
            model = model with { ShoppingCartItem = model.ShoppingCartItem.WithQuantity(minimum - item.Quantity) };
        }

        return model;
    }

    public Task<bool> IsApplicableAsync(ProductEstimationContext model) => Task.FromResult(true);
}
