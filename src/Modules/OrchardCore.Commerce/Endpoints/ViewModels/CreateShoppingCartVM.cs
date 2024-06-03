using OrchardCore.Commerce.AddressDataType;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class CreateShoppingCartVM
{
    public string shoppingCartId { get; set; }
    public Address shipping { get; set; }
    public Address billing { get; set; }
}
