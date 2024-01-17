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

    // If BillingAndShippingAddressesMatch is ticked, we return the billing address for the shipping address as well.
    public Address GetSafeShippingAddress() =>
        BillingAndShippingAddressesMatch.Value
            ? BillingAddress.Address
            : ShippingAddress.Address;

    public Address GetSafeBillingAddress() =>
        string.IsNullOrWhiteSpace(BillingAddress.Address.Name)
            ? ShippingAddress.Address
            : BillingAddress.Address;
}
