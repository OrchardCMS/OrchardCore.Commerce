using OrchardCore.Commerce.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

public class UserAddressesPart : ContentPart
{
    public AddressField ShippingAddress { get; set; }
    public AddressField BillingAddress { get; set; }
}
