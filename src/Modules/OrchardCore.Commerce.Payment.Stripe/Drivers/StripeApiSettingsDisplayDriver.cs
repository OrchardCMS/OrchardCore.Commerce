using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.Payment.Stripe.Extensions;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Commerce.Payment.Stripe.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Stripe.Drivers;

public class StripeApiSettingsDisplayDriver : SiteDisplayDriver<StripeApiSettings>
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
        ILogger<StripeApiSettingsDisplayDriver> logger
    )
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _hca = hca;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    protected override string SettingsGroupId => GroupId;

    public override async Task<IDisplayResult> EditAsync(
        ISite model,
        StripeApiSettings section,
        BuildEditorContext context
    )
    {
        if (!await AuthorizeAsync())
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<StripeApiSettingsViewModel>(
                "StripeApiSettings_Edit",
                model =>
                {
                    model.PublishableKey = section.PublishableKey;

                    model.AccountId = section.AccountId;

                    // Decrypting key.
                    model.SecretKey = section.DecryptSecretKey(_dataProtectionProvider, _logger);

                    model.WebhookSigningSecret = section.WebhookSigningSecret.DecryptStripeApiKey(
                        _dataProtectionProvider,
                        _logger
                    );
                }
            )
            .PlaceInContent()
            .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(
        ISite model,
        StripeApiSettings section,
        UpdateEditorContext context
    )
    {
        if (await context.CreateModelMaybeAsync<StripeApiSettingsViewModel>(Prefix, AuthorizeAsync) is { } viewModel)
        {
            var previousSecretKey = section.SecretKey;
            var previousWebhookKey = section.WebhookSigningSecret;
            section.PublishableKey = viewModel.PublishableKey?.Trim();

            // Restore secret key if the input is empty, meaning that it has not been reset.
            if (string.IsNullOrWhiteSpace(viewModel.SecretKey))
            {
                section.SecretKey = previousSecretKey;
            }
            else
            {
                // Encrypt secret key.
                var protector = _dataProtectionProvider.CreateProtector(
                    nameof(StripeApiSettingsConfiguration)
                );
                section.SecretKey = protector.Protect(viewModel.SecretKey?.Trim());
            }

            section.AccountId = string.IsNullOrWhiteSpace(viewModel.AccountId)
                ? null
                : viewModel.AccountId.Trim();

            if (string.IsNullOrWhiteSpace(viewModel.WebhookSigningSecret))
            {
                section.WebhookSigningSecret = previousWebhookKey;
            }
            else
            {
                var protector = _dataProtectionProvider.CreateProtector(
                    nameof(StripeApiSettingsConfiguration)
                );
                section.WebhookSigningSecret = protector.Protect(
                    viewModel.WebhookSigningSecret?.Trim()
                );
            }

            // Release the tenant to apply settings.
            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }

        return await EditAsync(model, section, context);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeAsync(
            _hca.HttpContext?.User,
            Permissions.ManageStripeApiSettings
        );
}
