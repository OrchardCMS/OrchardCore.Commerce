using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class PriceDisplaySettingsDisplayDriver : SiteDisplayDriver<PriceDisplaySettings>
{
    public const string GroupId = "PriceDisplay";

    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IHttpContextAccessor _hca;
    private readonly IAuthorizationService _authorizationService;

    public PriceDisplaySettingsDisplayDriver(
        IShellHost shellHost,
        ShellSettings shellSettings,
        IHttpContextAccessor hca,
        IAuthorizationService authorizationService)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _hca = hca;
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite model, PriceDisplaySettings section, BuildEditorContext context)
    {
        var user = _hca.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManagePriceDisplaySettings))
        {
            return null;
        }

        context.Shape.AddTenantReloadWarning();

        return Initialize<PriceDisplaySettingsViewModel>("PriceDisplaySettings_Edit", model =>
        {
            model.UseNetPriceDisplay = section.UseNetPriceDisplay;
            model.UseGrossPriceDisplay = section.UseGrossPriceDisplay;
        })
            .PlaceInContent()
            .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite model, PriceDisplaySettings section, UpdateEditorContext context)
    {
        var user = _hca.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManagePriceDisplaySettings))
        {
            return null;
        }

        var viewModel = new PriceDisplaySettingsViewModel();
        if (context.GroupId == GroupId && await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            section.UseNetPriceDisplay = viewModel.UseNetPriceDisplay;
            section.UseGrossPriceDisplay = viewModel.UseGrossPriceDisplay;

            // Release the tenant to apply settings.
            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }

        return await EditAsync(model, section, context);
    }
}
