using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Payment.Exactly.Services;

public class ExactlySettingsConfiguration : IConfigureOptions<ExactlySettings>
{
    private readonly ISiteService _siteService;

    public ExactlySettingsConfiguration(ISiteService siteService) => _siteService = siteService;

    public void Configure(ExactlySettings options)
    {
        var settings = _siteService
            .GetSiteSettings()
            .GetOrCreate<ExactlySettings>();

        settings.MigrateLegacyKeys();
        options.Production = new ExactlyEnvironmentSettings();
        settings.Production?.CopyTo(options.Production);
        options.Sandbox = new ExactlyEnvironmentSettings();
        settings.Sandbox?.CopyTo(options.Sandbox);
        options.ClearLegacyKeys();
    }
}
