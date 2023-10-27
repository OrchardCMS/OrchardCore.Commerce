using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce;

public class Permissions : AdminPermissionBase
{
    public static readonly Permission ManageCurrencySettings = new(nameof(ManageCurrencySettings), "Manage Currency Settings");
    public static readonly Permission ManagePriceDisplaySettings = new(nameof(ManagePriceDisplaySettings), "Manage Price Display Settings");
    public static readonly Permission ManageRegionSettings = new(nameof(ManageRegionSettings), "Manage Region Settings");

    private static readonly IReadOnlyList<Permission> _adminPermissions = new[]
    {
        ManageCurrencySettings,
        ManageRegionSettings,
        ManagePriceDisplaySettings,
    };

    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
