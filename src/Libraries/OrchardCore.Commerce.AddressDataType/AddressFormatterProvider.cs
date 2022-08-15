using OrchardCore.Commerce.AddressDataType.Abstractions;

namespace OrchardCore.Commerce.AddressDataType;

public class AddressFormatterProvider : IAddressFormatterProvider
{
    public string Format(Address address)
    {
        var region = address?.Region;
        if (region != null &&
            Regions.RegionCodes.TryGetValue(region, out var regionCode) &&
            KnownAddressFormatters.Formatters.TryGetValue(regionCode, out var formatter))
        {
            return formatter.Format(address);
        }

        return KnownAddressFormatters.DefaultFormatter.Format(address);
    }
}
