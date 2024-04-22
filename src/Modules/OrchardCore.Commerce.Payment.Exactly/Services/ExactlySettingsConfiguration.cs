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
        var siteSettings = _siteService
            .GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<ExactlySettings>();

        siteSettings.CopyTo(options);
    }
}
