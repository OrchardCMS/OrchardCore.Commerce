using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Payment.Stripe.Services;

public class StripeApiSettingsConfiguration : IConfigureOptions<StripeApiSettings>
{
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public StripeApiSettingsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<StripeApiSettingsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(StripeApiSettings options)
    {
        var settings = _siteService
            .GetSiteSettings()
            .GetOrCreate<StripeApiSettings>();

        settings.MigrateLegacyKeys();
        options.Production = settings.Production ?? new StripeApiEnvironmentSettings();
        options.Sandbox = settings.Sandbox ?? new StripeApiEnvironmentSettings();
        options.ClearLegacyKeys();

        options.Production.SecretKey = options.Production.DecryptSecretKey(_dataProtectionProvider, _logger);
        options.Sandbox.SecretKey = options.Sandbox.DecryptSecretKey(_dataProtectionProvider, _logger);
    }
}
