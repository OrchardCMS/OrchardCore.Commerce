using System;

namespace InternationalAddress
{
    /// <summary>
    /// Formats an address.
    /// </summary>
    public interface IAddressFormatter
    {
        string Format(Address address)
            => (address.Name
            + (String.IsNullOrWhiteSpace(address.Department) ? "" : Environment.NewLine + address.Department)
            + (String.IsNullOrWhiteSpace(address.Company) ? "" : Environment.NewLine + address.Company)
            + Environment.NewLine + address.StreetAddress1
            + (String.IsNullOrWhiteSpace(address.StreetAddress2) ? "" : Environment.NewLine + address.StreetAddress2)
            + Environment.NewLine + address.City
            + (String.IsNullOrWhiteSpace(address.Province) ? "" : " " + address.Province)
            + " " + address.PostalCode
            + (String.IsNullOrWhiteSpace(address.Country) ? "" : Environment.NewLine + address.Country)
            ).ToUpper();
    }
}
