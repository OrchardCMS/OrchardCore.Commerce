using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Commerce.Models;

public class UserAddressesPart : ContentPart
{
    public AddressField ShippingAddress { get; set; } = new();
    public AddressField BillingAddress { get; set; } = new();
    public BooleanField BillingAndShippingAddressesMatch { get; set; } = new();

    [SuppressMessage(
        "Design",
        "CA1024:Use properties where appropriate",
        Justification = "It's not appropriate for it's counterpart for billing so this should remain a method for parity")]
    public Address GetSafeShippingAddress() =>
        // If BillingAndShippingAddressesMatch is ticked, we return the billing address for the shipping address as well.
        BillingAndShippingAddressesMatch.Value ? BillingAddress.Address : ShippingAddress.Address;

    public Address GetSafeBillingAddress() =>
        string.IsNullOrWhiteSpace(BillingAddress.Address.Name) ? ShippingAddress.Address : BillingAddress.Address;
}
