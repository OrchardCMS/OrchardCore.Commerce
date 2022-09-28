using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OrchardCore.Commerce.Extensions;

public static class RegionExtensions
{
    public static IEnumerable<SelectListItem> CreateSelectListOptions(this IEnumerable<RegionInfo> regionInfos) =>
        regionInfos.OrderBy(region => region.DisplayName).Select(region => new SelectListItem(
            region.DisplayName,
            region.TwoLetterISORegionName));
}
