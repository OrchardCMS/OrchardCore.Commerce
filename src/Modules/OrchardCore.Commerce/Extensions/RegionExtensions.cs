using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Commerce.AddressDataType;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Commerce.Extensions;

public static class RegionExtensions
{
    public static IEnumerable<SelectListItem> CreateSelectListOptions(this IEnumerable<Region> regionInfos) =>
        regionInfos.OrderBy(region => region.EnglishName).Select(region => new SelectListItem(
            region.EnglishName,
            region.TwoLetterISORegionName));
}
