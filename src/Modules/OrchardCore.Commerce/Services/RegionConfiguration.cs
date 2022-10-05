using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Models;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Services;

public class RegionSettingsConfiguration : IConfigureOptions<RegionSettings>
{
    private readonly ISiteService _siteService;

    public RegionSettingsConfiguration(ISiteService siteService) =>
        _siteService = siteService;

    public void Configure(RegionSettings options)
    {
        var settings = _siteService
            .GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<RegionSettings>();

        options.AllowedRegions = settings.AllowedRegions;
    }
}
