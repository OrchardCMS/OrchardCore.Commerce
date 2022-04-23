using System;

namespace InternationalAddress;

/// <summary>
/// Formats an address.
/// </summary>
public interface IAddressFormatter
{
    string Format(Address address)
        => address is null ? "-" : (address.Name
                                    + (String.IsNullOrWhiteSpace(address.Department) ? string.Empty : Environment.NewLine + address.Department)
                                    + (String.IsNullOrWhiteSpace(address.Company) ? string.Empty : Environment.NewLine + address.Company)
                                    + Environment.NewLine + address.StreetAddress1
                                    + (String.IsNullOrWhiteSpace(address.StreetAddress2) ? string.Empty : Environment.NewLine + address.StreetAddress2)
                                    + Environment.NewLine + address.City
                                    + (String.IsNullOrWhiteSpace(address.Province) ? string.Empty : " " + address.Province)
                                    + " " + address.PostalCode
                                    + (String.IsNullOrWhiteSpace(address.Region) ? string.Empty : Environment.NewLine + address.Region)
            ).ToUpper();
}