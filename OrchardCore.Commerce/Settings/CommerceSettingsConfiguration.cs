using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Settings
{
    public class CommerceSettingsConfiguration : IConfigureOptions<CommerceSettings>
    {
        private readonly ISiteService _site;
        private readonly ILogger<CommerceSettingsConfiguration> _logger;

        public CommerceSettingsConfiguration(
            ISiteService site,
            ILogger<CommerceSettingsConfiguration> logger)
        {
            _site = site;
            _logger = logger;
        }

        public void Configure(CommerceSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<CommerceSettings>();

            options.DefaultCurrency = settings.DefaultCurrency;
            options.CurrentDisplayCurrency = settings.CurrentDisplayCurrency;
        }
    }
}
