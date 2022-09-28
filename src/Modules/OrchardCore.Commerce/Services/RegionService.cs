using GraphQL;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Settings;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;
public class RegionService : IRegionService
{
    private readonly ISiteService _siteService;

    public RegionService(ISiteService siteService) =>
        _siteService = siteService;

    public async Task<IEnumerable<RegionInfo>> GetAvailableRegionsAsync() =>
        (await _siteService.GetSiteSettingsAsync())
            .As<RegionSettings>().AllowedRegions.GetRegionInfosFromTwoLetterRegionIsos();
}
