using OrchardCore.Commerce.ViewModels;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class AddItemViewModel
{
    public string Token { get; set; }
    public ShoppingCartLineUpdateModel Line { get; set; }
}
