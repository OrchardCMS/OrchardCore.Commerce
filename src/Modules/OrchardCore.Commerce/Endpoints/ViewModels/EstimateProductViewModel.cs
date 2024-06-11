using OrchardCore.Commerce.AddressDataType;

namespace OrchardCore.Commerce.Endpoints.ViewModels;
public class EstimateProductViewModel
{
    public string Sku { get; set; }
    public Address Shipping { get; set; }
    public Address Billing { get; set; }
}
