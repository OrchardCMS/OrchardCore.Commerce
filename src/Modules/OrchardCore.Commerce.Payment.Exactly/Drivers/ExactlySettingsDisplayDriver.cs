using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Payment.Exactly.Drivers;

public class ExactlySettingsDisplayDriver : SiteDisplayDriver<ExactlySettings>
{
    public const string EditorGroupId = "Exactly";
    public const string SignUpLink = "https://application.exactly.com/?utm_source=partner&utm_medium=kirill&utm_campaign=LOMBIQ";

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _hca;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly ExactlySettings _ssoSettings;

    protected override string SettingsGroupId => EditorGroupId;

    public ExactlySettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor hca,
        IShellReleaseManager shellReleaseManager,
        IOptionsSnapshot<ExactlySettings> ssoSettings)
    {
        _authorizationService = authorizationService;
        _hca = hca;
        _shellReleaseManager = shellReleaseManager;
        _ssoSettings = ssoSettings.Value;
    }

    public override async Task<IDisplayResult> EditAsync(ISite model, ExactlySettings section, BuildEditorContext context)
    {
        if (!await AuthorizeAsync()) return null;

        context.AddTenantReloadWarningWrapper();

        return Initialize<ExactlySettings>($"{nameof(ExactlySettings)}_Edit", settings =>
            {
                _ssoSettings.CopyTo(settings);
                settings.ApiKey = string.Empty;
            })
            .PlaceInContent()
            .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite model, ExactlySettings section, UpdateEditorContext context)
    {
        if (await context.CreateModelMaybeAsync<ExactlySettings>(Prefix, AuthorizeAsync) is not { } viewModel)
        {
            return null;
        }

        viewModel.CopyTo(section);

        // Release the tenant to apply settings.
        _shellReleaseManager.RequestRelease();

        return await EditAsync(model, section, context);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeCurrentUserAsync(_hca.HttpContext, Permissions.ManageExactlySettings);
}
