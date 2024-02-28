using OrchardCore.Commerce.AddressDataType.Abstractions;
using System;
using static OrchardCore.Commerce.AddressDataType.ConcatenationHelper;

namespace OrchardCore.Commerce.AddressDataType;

public class DefaultAddressFormatter : IAddressFormatter
{
    public string Format(Address address) =>
        address is null
        ? "-"
        : JoinNotNullAndNotWhiteSpace(
            Environment.NewLine,
            address.Department,
            address.Company,
            address.StreetAddress1,
            address.StreetAddress2,
            JoinNotNullAndNotWhiteSpace(separator: " ", address.City, address.Province, address.PostalCode),
            address.Region).ToUpperInvariant();
}
