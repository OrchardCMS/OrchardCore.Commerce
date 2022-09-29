using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.Commerce.AddressDataType;
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

public class RegionSettingsDisplayDriver : SectionDisplayDriver<ISite, RegionSettings>
{
    public const string GroupId = "Region";

    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IHttpContextAccessor _hca;
    private readonly IAuthorizationService _authorizationService;

    public RegionSettingsDisplayDriver(
        IShellHost shellHost,
        ShellSettings shellSettings,
        IHttpContextAccessor hca,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<RegionSettingsDisplayDriver> logger)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _hca = hca;
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(RegionSettings section, BuildEditorContext context)
    {
        var user = _hca.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageRegionSettings))
        {
            return null;
        }

        return Initialize<RegionSettingsViewModel>("RegionSettings_Edit", model =>
        {
            model.AllowedRegions = section.AllowedRegions;

            model.Regions = Regions.All.AsEnumerable().CreateSelectListOptions();
        })
            .Location("Content")
            .OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(RegionSettings section, BuildEditorContext context)
    {
        var user = _hca.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageRegionSettings))
        {
            return null;
        }

        if (context.GroupId == GroupId)
        {
            var model = new RegionSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                var allowedRegions = model.AllowedRegions;

                var allRegionTwoLetterIsoRegionNames = Regions.All.Select(region => region.TwoLetterISORegionName);

                section.AllowedRegions = allowedRegions != null && allowedRegions.Any()
                    ? allRegionTwoLetterIsoRegionNames
                        .Where(regionTwoLetterIsoRegionName => allowedRegions.Contains(regionTwoLetterIsoRegionName))
                    : allRegionTwoLetterIsoRegionNames;

                // Release the tenant to apply settings.
                await _shellHost.ReleaseShellContextAsync(_shellSettings);
            }
        }

        return await EditAsync(section, context);
    }
}
