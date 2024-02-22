namespace OrchardCore.Commerce.AddressDataType.Abstractions;

/// <summary>
/// Formats an address.
/// </summary>
public interface IAddressFormatter
{
    /// <summary>
    /// Returns a multi-line formatted string representing the <paramref name="address"/>.
    /// </summary>
    string Format(Address address);
}
