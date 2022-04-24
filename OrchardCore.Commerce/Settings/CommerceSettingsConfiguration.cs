using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Settings;

public class CommerceSettingsConfiguration : IConfigureOptions<CommerceSettings>
{
    private readonly ISiteService _site;

    public CommerceSettingsConfiguration(ISiteService site) => _site = site;

    public void Configure(CommerceSettings options)
    {
        var settings = _site.GetSiteSettingsAsync()
            .GetAwaiter().GetResult()
            .As<CommerceSettings>();

        options.DefaultCurrency = settings.DefaultCurrency;
        options.CurrentDisplayCurrency = settings.CurrentDisplayCurrency;
    }
}
