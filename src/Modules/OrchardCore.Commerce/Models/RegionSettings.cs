using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Commerce.Models;

public class RegionSettings
{
    public IEnumerable<RegionInfo> AllowedRegions { get; set; }
}
