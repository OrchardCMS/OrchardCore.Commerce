using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.Commerce.Payment.Exactly.ViewModels;
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

    protected override string SettingsGroupId => EditorGroupId;

    public ExactlySettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor hca,
        IShellReleaseManager shellReleaseManager)
    {
        _authorizationService = authorizationService;
        _hca = hca;
        _shellReleaseManager = shellReleaseManager;
    }

    public override async Task<IDisplayResult> EditAsync(ISite model, ExactlySettings section, BuildEditorContext context)
    {
        if (!await AuthorizeAsync()) return null;

        context.AddTenantReloadWarningWrapper();
        section.MigrateLegacyKeys();

        return Initialize<ExactlySettingsViewModel>($"{nameof(ExactlySettings)}_Edit", settings =>
            {
                MapToViewModel(section.Production, settings.Production);
                MapToViewModel(section.Sandbox, settings.Sandbox);
            })
            .PlaceInContent()
            .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite model, ExactlySettings section, UpdateEditorContext context)
    {
        if (await context.CreateModelMaybeAsync<ExactlySettingsViewModel>(Prefix, AuthorizeAsync) is not { } viewModel)
        {
            return null;
        }

        section.MigrateLegacyKeys();
        viewModel.Production?.CopyTo(section.Production ??= new ExactlyEnvironmentSettings());
        viewModel.Sandbox?.CopyTo(section.Sandbox ??= new ExactlyEnvironmentSettings());
        section.ClearLegacyKeys();

        // Release the tenant to apply settings.
        _shellReleaseManager.RequestRelease();

        return await EditAsync(model, section, context);
    }

    private static void MapToViewModel(ExactlyEnvironmentSettings section, ExactlyEnvironmentSettings model)
    {
        section ??= new ExactlyEnvironmentSettings();
        model.BaseAddress = section.BaseAddress;
        model.ProjectId = section.ProjectId;
        model.ApiKey = string.Empty;
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeCurrentUserAsync(_hca.HttpContext, Permissions.ManageExactlySettings);
}
