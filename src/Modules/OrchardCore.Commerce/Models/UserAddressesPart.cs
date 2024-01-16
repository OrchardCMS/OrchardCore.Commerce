using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System;

namespace OrchardCore.Commerce.Models;

public class UserAddressesPart : ContentPart
{
    public AddressField ShippingAddress { get; set; } = new();
    public AddressField BillingAddress { get; set; } = new();
    public BooleanField BillingAndShippingAddressesMatch { get; set; } = new();

    public Address GetShippingAddress() =>
        string.IsNullOrWhiteSpace(ShippingAddress.Address.Name)
            ? BillingAddress.Address
            : ShippingAddress.Address;

    public Address GetBillingAddress() =>
        BillingAndShippingAddressesMatch.Value
            ? GetShippingAddress()
            : BillingAddress.Address;
}
