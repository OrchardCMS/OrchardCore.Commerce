using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class StripeApiSettingsDisplayDriver : SectionDisplayDriver<ISite, StripeApiSettings>
{
    public const string GroupId = "StripeApi";
    private readonly ILogger _logger;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IHttpContextAccessor _hca;
    private readonly IAuthorizationService _authorizationService;

    public StripeApiSettingsDisplayDriver(
        IShellHost shellHost,
        ShellSettings shellSettings,
        IHttpContextAccessor hca,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<StripeApiSettingsDisplayDriver> logger)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _hca = hca;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public override async Task<IDisplayResult> EditAsync(StripeApiSettings section, BuildEditorContext context)
    {
        var user = _hca.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageStripeApiSettings))
        {
            return null;
        }

        return Initialize<StripeApiSettingsViewModel>("StripeApiSettings_Edit", model =>
            {
                model.PublishableKey = section.PublishableKey;

                // Decrypting key.
                model.SecretKey = section.DecryptSecretKey(_dataProtectionProvider, _logger);

                model.WebhookSigningSecret = section.WebhookSigningSecret.DecryptStripeApiKey(_dataProtectionProvider, _logger);
            })
            .Location("Content")
            .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(StripeApiSettings section, BuildEditorContext context)
    {
        var user = _hca.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageStripeApiSettings))
        {
            return null;
        }

        if (context.GroupId == GroupId)
        {
            var model = new StripeApiSettingsViewModel();
            var previousSecretKey = section.SecretKey;
            var previousWebhookKey = section.WebhookSigningSecret;

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                section.PublishableKey = model.PublishableKey?.Trim();

                // Restore secret key if the input is empty, meaning that it has not been reset.
                if (string.IsNullOrWhiteSpace(model.SecretKey))
                {
                    section.SecretKey = previousSecretKey;
                }
                else
                {
                    // Encrypt secret key.
                    var protector = _dataProtectionProvider.CreateProtector(nameof(StripeApiSettingsConfiguration));
                    section.SecretKey = protector.Protect(model.SecretKey?.Trim());
                }

                if (string.IsNullOrWhiteSpace(model.WebhookSigningSecret))
                {
                    section.WebhookSigningSecret = previousWebhookKey;
                }
                else
                {
                    var protector = _dataProtectionProvider.CreateProtector(nameof(StripeApiSettingsConfiguration));
                    section.WebhookSigningSecret = protector.Protect(model.WebhookSigningSecret?.Trim());
                }

                // Release the tenant to apply settings.
                await _shellHost.ReleaseShellContextAsync(_shellSettings);
            }
        }

        return await EditAsync(section, context);
    }
}
