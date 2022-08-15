using System;
using static OrchardCore.Commerce.AddressDataType.ConcatenationHelper;

namespace OrchardCore.Commerce.AddressDataType.Abstractions;

/// <summary>
/// Formats an address.
/// </summary>
public interface IAddressFormatter
{
    /// <summary>
    /// Returns a multi-line formatted string representing the <paramref name="address"/>.
    /// </summary>
    string Format(Address address) =>
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
