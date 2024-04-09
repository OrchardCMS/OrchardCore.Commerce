using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Drivers;

public class ExactlySettingsDisplayDriver : SectionDisplayDriver<ISite, ExactlySettings>
{
    public const string EditorGroupId = "Exactly";

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly ExactlySettings _ssoSettings;

    public ExactlySettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor hca,
        IShellHost shellHost,
        ShellSettings shellSettings,
        IOptionsSnapshot<ExactlySettings> ssoSettings)
    {
        _authorizationService = authorizationService;
        _hca = hca;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _ssoSettings = ssoSettings.Value;
    }

    public override async Task<IDisplayResult> EditAsync(ExactlySettings section, BuildEditorContext context)
    {
        if (!context.GroupId.EqualsOrdinalIgnoreCase(EditorGroupId) || !await AuthorizeAsync()) return null;

        context.Shape.AddTenantReloadWarning();

        return Initialize<ExactlySettings>($"{nameof(ExactlySettings)}_Edit", settings =>
            {
                _ssoSettings.CopyTo(settings);
                settings.ApiKey = string.Empty;
            })
            .PlaceInContent()
            .OnGroup(EditorGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ExactlySettings section, BuildEditorContext context)
    {
        var viewModel = new ExactlySettings();

        if (context.GroupId == EditorGroupId &&
            await AuthorizeAsync() &&
            await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            viewModel.CopyTo(section);

            // Release the tenant to apply settings.
            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }

        return await EditAsync(section, context);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeCurrentUserAsync(_hca.HttpContext, Permissions.ManageExactlySettings);
}
