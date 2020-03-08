namespace InternationalAddress
{
    public class AddressFormatterProvider : IAddressFormatterProvider
    {
        public string Format(Address address, string countryCode)
            => KnownAddressFormatters.Formatters.TryGetValue(countryCode, out var formatter)
            ? formatter.Format(address)
            : KnownAddressFormatters.DefaultFormatter.Format(address);
    }
}
