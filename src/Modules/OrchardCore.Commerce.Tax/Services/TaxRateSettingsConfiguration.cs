using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Tax.Services;

public class TaxRateSettingsConfiguration : IConfigureOptions<TaxRateSettings>
{
    private readonly ISiteService _siteService;

    public TaxRateSettingsConfiguration(ISiteService siteService) => _siteService = siteService;

    public void Configure(TaxRateSettings options)
    {
        var settings = _siteService
            .GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<TaxRateSettings>();

        options.CopyFrom(settings);
    }
}
