using OrchardCore.Commerce.Fields;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models;

public class UserAddressesPart : ContentPart
{
    public AddressField ShippingAddress { get; set; } = new();
    public AddressField BillingAddress { get; set; } = new();
    public BooleanField BillingAndShippingAddressesMatch { get; set; } = new();
}
