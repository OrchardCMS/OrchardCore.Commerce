using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;
public class RegionService : IRegionService
{
    private readonly ISiteService _siteService;

    public RegionService(ISiteService siteService) =>
        _siteService = siteService;

    public async Task<IEnumerable<Region>> GetAvailableRegionsAsync() =>
        (await _siteService.GetSiteSettingsAsync()).As<RegionSettings>()?.AllowedRegions is { } regions &&
        regions.Any()
            ? regions.GetRegionInfosFromTwoLetterRegionIsos()
            : Regions.All;
}
