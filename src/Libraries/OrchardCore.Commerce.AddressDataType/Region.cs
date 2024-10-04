using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.AddressDataType;

public record Region(string EnglishName, string TwoLetterISORegionName, string DisplayName)
{
    [Obsolete("Only serializers should use this.")]
    [JsonConstructor]
    private Region()
        : this(string.Empty, string.Empty, string.Empty)
    { }

    public Region(RegionInfo info)
        : this(info.EnglishName, info.TwoLetterISORegionName, info.EnglishName)
    { }
}
