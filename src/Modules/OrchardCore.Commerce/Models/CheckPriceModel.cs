using OrchardCore.Commerce.AddressDataType;

namespace OrchardCore.Commerce.Models;

public class CheckPriceModel
{
    public Address ShippingAddress { get; set; }
    public Address BillingAddress { get; set; }
}
