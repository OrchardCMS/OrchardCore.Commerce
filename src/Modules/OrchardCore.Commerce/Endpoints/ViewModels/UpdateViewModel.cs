using OrchardCore.Commerce.ViewModels;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class UpdateViewModel
{
    public string Token { get; set; }
    public string ShoppingCartId { get; set; }
    public ShoppingCartUpdateModel Cart { get; set; }
}
