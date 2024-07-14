using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Extensions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Drivers;

public class RegionSettingsDisplayDriver : SectionDisplayDriver<ISite, RegionSettings>
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

    public override async Task<IDisplayResult> EditAsync(RegionSettings section, BuildEditorContext context)
    {
        if (!GroupId.EqualsOrdinalIgnoreCase(context.GroupId) || !await AuthorizeAsync())
        {
            return null;
        }

        context.Shape.AddTenantReloadWarning();

        return Initialize<RegionSettingsViewModel>("RegionSettings_Edit", model =>
            {
                model.AllowedRegions = section.AllowedRegions;
                model.Regions = _regionService
                    .GetAllRegions()
                    .CreateSelectListOptions();
            })
            .PlaceInContent()
            .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(RegionSettings section, UpdateEditorContext context)
    {
        if (await context.CreateModelMaybeAsync<RegionSettingsViewModel>(Prefix, GroupId, AuthorizeAsync) is { } model)
        {
            var allowedRegions = model.AllowedRegions?.AsList() ?? [];
            var allRegionTwoLetterIsoRegionNames = _regionService
                .GetAllRegions()
                .Select(region => region.TwoLetterISORegionName);

            section.AllowedRegions = allowedRegions.Count != 0
                ? allRegionTwoLetterIsoRegionNames.Where(allowedRegions.Contains)
                : allRegionTwoLetterIsoRegionNames;

            // Release the tenant to apply settings.
            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }

        return await EditAsync(section, context);
    }

    private Task<bool> AuthorizeAsync() =>
        _authorizationService.AuthorizeAsync(_hca.HttpContext?.User, Permissions.ManageRegionSettings);
}
