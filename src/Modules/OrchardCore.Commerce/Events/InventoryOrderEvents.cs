using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class InventoryOrderEvents : IOrderEvents
{
    private readonly IProductInventoryService _productInventoryService;
    private readonly IShoppingCartHelpers _shoppingCartHelpers;

    public InventoryOrderEvents(
        IProductInventoryService productInventoryService,
        IShoppingCartHelpers shoppingCartHelpers)
    {
        _productInventoryService = productInventoryService;
        _shoppingCartHelpers = shoppingCartHelpers;
    }

    public async Task OrderedAsync(ContentItem order, string shoppingCartId)
    {
        var cart = await _shoppingCartHelpers.RetrieveAsync(shoppingCartId);

        // Decrease inventories of purchased items.
        await _productInventoryService.UpdateInventoriesAsync(cart.Items);
    }
}
