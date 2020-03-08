namespace InternationalAddress
{
    /// <summary>
    /// A service that selects the right address formatter for a culture.
    /// </summary>
    public interface IAddressFormatterProvider
    {
        /// <summary>
        /// Format the address for this culture.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="countryCode">The ISO code for the country.</param>
        /// <returns>The formatted address.</returns>
        string Format(Address address, string countryCode);
    }
}
