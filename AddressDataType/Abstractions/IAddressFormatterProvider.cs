namespace InternationalAddress
{
    /// <summary>
    /// A service that selects the right address formatter for a culture.
    /// </summary>
    public interface IAddressFormatterProvider
    {
        /// <summary>
        /// Format the address for its country's standard.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>The formatted address.</returns>
        string Format(Address address);
    }
}
