using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Settings;

public class CurrencySettingsConfiguration : IConfigureOptions<CurrencySettings>
{
    private readonly ISiteService _site;

    public CurrencySettingsConfiguration(ISiteService site) => _site = site;

    public void Configure(CurrencySettings options)
    {
        var settings = _site
            .GetSiteSettingsAsync()
            .GetAwaiter()
            .GetResult()
            .As<CurrencySettings>();

        options.DefaultCurrency = settings.DefaultCurrency;
        options.CurrentDisplayCurrency = settings.CurrentDisplayCurrency;
    }
}
