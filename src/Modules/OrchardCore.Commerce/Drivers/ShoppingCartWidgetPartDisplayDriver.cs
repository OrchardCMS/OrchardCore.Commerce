using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class ShoppingCartWidgetPartDisplayDriver : ContentPartDisplayDriver<ShoppingCartWidgetPart>
{
    private readonly IShoppingCartPersistence _shoppingCartPersistence;

    public ShoppingCartWidgetPartDisplayDriver(IShoppingCartPersistence shoppingCartPersistence) =>
        _shoppingCartPersistence = shoppingCartPersistence;

    public override IDisplayResult Display(ShoppingCartWidgetPart part, BuildPartDisplayContext context) =>
        Initialize<ShoppingCartWidgetPartViewModel>(GetDisplayShapeType(context), PopulateViewModelAsync)
            .Location("Detail", "Content:25")
            .Location("Summary", "Meta:10");

    private async ValueTask PopulateViewModelAsync(ShoppingCartWidgetPartViewModel model)
    {
        // Shopping cart ID is null by default currently.
        var cart = await _shoppingCartPersistence.RetrieveAsync();

        model.ItemCount = cart?.ItemCount ?? 0;
    }
}
