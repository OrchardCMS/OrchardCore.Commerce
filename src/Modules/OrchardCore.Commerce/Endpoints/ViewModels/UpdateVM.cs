using OrchardCore.Commerce.ViewModels;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class UpdateVM
{
    public string Token { get; set; }
    public string ShoppingCartId { get; set; }
    public ShoppingCartUpdateModel Cart { get; set; }
}
