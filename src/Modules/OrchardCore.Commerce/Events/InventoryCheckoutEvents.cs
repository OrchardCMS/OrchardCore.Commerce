using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class InventoryCheckoutEvents : ICheckoutEvents
{
    private readonly IProductInventoryService _productInventoryService;

    public InventoryCheckoutEvents(
        IProductInventoryService productInventoryService) => _productInventoryService = productInventoryService;

    public async Task ViewModelCreatedAsync(IList<ShoppingCartLineViewModel> lines, ICheckoutViewModel checkoutViewModel)
    {
        var cannotCheckout = await _productInventoryService.VerifyLinesAsync(lines);
        if (cannotCheckout)
        {
            checkoutViewModel.IsInvalid = true;
        }

        return;
    }
}
