namespace InternationalAddress
{
    public class AddressFormatterProvider : IAddressFormatterProvider
    {
        public string Format(Address address)
        {
            string region = address?.Region;
            if (region != null
                && Regions.RegionCodes.TryGetValue(region, out string regionCode)
                && KnownAddressFormatters.Formatters.TryGetValue(regionCode, out var formatter))
            {
                return formatter.Format(address);
            }
            return KnownAddressFormatters.DefaultFormatter.Format(address);
        }
    }
}
