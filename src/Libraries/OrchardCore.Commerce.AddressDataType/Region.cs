using System.Globalization;

namespace OrchardCore.Commerce.AddressDataType;

public record Region(string EnglishName, string TwoLetterISORegionName)
{
    public Region(RegionInfo info)
        : this(info.EnglishName, info.TwoLetterISORegionName)
    { }
}
