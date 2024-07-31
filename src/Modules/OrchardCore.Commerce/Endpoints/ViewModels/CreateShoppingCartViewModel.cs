using OrchardCore.Commerce.AddressDataType;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class CreateShoppingCartViewModel
{
    public string ShoppingCartId { get; set; }
    public Address Shipping { get; set; }
    public Address Billing { get; set; }
}
