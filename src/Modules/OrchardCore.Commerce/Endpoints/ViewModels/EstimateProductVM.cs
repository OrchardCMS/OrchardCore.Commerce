using OrchardCore.Commerce.AddressDataType;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class EstimateProductVM
{
    public string Sku { get; set; }
    public string ShoppingCartId { get; set; }
    public Address? Shipping { get; set; }
    public Address? Billing { get; set; }
}
