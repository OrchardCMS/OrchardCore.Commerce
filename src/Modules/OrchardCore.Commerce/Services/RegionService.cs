using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.AddressDataType;
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
    private readonly IStringLocalizer<RegionService> T;

    public RegionService(ISiteService siteService, IStringLocalizer<RegionService> stringLocalizer)
    {
        _siteService = siteService;
        T = stringLocalizer;
    }

    public IEnumerable<Region> GetAllRegions() =>
        Regions.All.Select(region => region with { DisplayName = T[region.EnglishName] });

    public async Task<IEnumerable<Region>> GetAvailableRegionsAsync()
    {
        var settings = await _siteService.GetSiteSettingsAsync();
        var allowedRegionCodes = (settings.As<RegionSettings>()?.AllowedRegions ?? Enumerable.Empty<string>()).AsList();

        var allRegions = GetAllRegions();

        if (!allowedRegionCodes.Any()) return allRegions;

        return allRegions.Where(region => allowedRegionCodes.Contains(region.TwoLetterISORegionName));
    }
}
