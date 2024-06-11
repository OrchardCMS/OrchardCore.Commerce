using OrchardCore.Commerce.ViewModels;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class RemoveLineViewModel
{
    public string ShoppingCartId { get; set; }
    public ShoppingCartLineUpdateModel Line { get; set; }
}
