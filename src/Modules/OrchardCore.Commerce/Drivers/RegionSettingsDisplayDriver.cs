using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class RegionSettingsDisplayDriver : SiteDisplayDriver<RegionSettings>
{
    public const string GroupId = "Region";

    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IHttpContextAccessor _hca;
    private readonly IAuthorizationService _authorizationService;
    private readonly IRegionService _regionService;

    public RegionSettingsDisplayDriver(
        IShellHost shellHost,
        ShellSettings shellSettings,
        IHttpContextAccessor hca,
        IAuthorizationService authorizationService,
        IRegionService regionService)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _hca = hca;
        _authorizationService = authorizationService;
        _regionService = regionService;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite model, RegionSettings section, BuildEditorContext context)
    {
        if (!await AuthorizeAsync())
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<RegionSettingsViewModel>("RegionSettings_Edit", model =>
            {
                model.AllowedRegions = section.AllowedRegions;
                model.Regions = _regionService
                    .GetAllRegions()
                    .CreateSelectListOptions();
            })
            .PlaceInContent()
            .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite model, RegionSettings section, UpdateEditorContext context)
    {
        if (await context.CreateModelMaybeAsync<RegionSettingsViewModel>(Prefix, AuthorizeAsync) is { } viewModel)
        {
            var allowedRegions = viewModel.AllowedRegions?.AsList() ?? [];
            var allRegionTwoLetterIsoRegionNames = _regionService
                .GetAllRegions()
                .Select(region => region.TwoLetterISORegionName);

            section.AllowedRegions = allowedRegions.Count != 0
                ? allRegionTwoLetterIsoRegionNames.Where(allowedRegions.Contains)
                : allRegionTwoLetterIsoRegionNames;

            // Release the tenant to apply settings.
            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }

        return await EditAsync(model, section, context);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeAsync(_hca.HttpContext?.User, Permissions.ManageRegionSettings);
}
