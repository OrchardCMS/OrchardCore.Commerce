using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce;

public class Permissions : AdminPermissionBase
{
    public static readonly Permission ManageCurrencySettings = new(nameof(ManageCurrencySettings), "Manage Currency Settings");
    public static readonly Permission ManagePriceDisplaySettings = new(nameof(ManagePriceDisplaySettings), "Manage Price Display Settings");
    public static readonly Permission ManageRegionSettings = new(nameof(ManageRegionSettings), "Manage Region Settings");
    public static readonly Permission ManageOrders = new(nameof(ManageOrders), "Manage Orders");
    public static readonly Permission Checkout = new(nameof(Checkout), "Ability to checkout");

    private static readonly IReadOnlyList<Permission> _adminPermissions = new[]
    {
        ManageCurrencySettings,
        Checkout,
        ManageRegionSettings,
        ManageOrders,
        ManagePriceDisplaySettings,
    };

    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
