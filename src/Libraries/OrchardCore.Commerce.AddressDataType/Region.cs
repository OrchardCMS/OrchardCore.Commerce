using System.Globalization;

namespace OrchardCore.Commerce.AddressDataType;

public record Region(string EnglishName, string TwoLetterISORegionName, string DisplayName)
{
    // This used to be a constructor, but has been turned into a factory method to avoid causing JSON serialization
    // exceptions. See https://github.com/dotnet/runtime/issues/45373#issuecomment-812091894 for more info on the
    // problem in general.
    public static Region FromRegionInfo(RegionInfo info) =>
        new(info.EnglishName, info.TwoLetterISORegionName, info.EnglishName);
}
