using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment;

public class Permissions : AdminPermissionBase
{
    public static readonly Permission ManageOrders = new(nameof(ManageOrders), "Manage Orders");
    public static readonly Permission Checkout = new(nameof(Checkout), "Ability to checkout");

    private static readonly IReadOnlyList<Permission> _adminPermissions = [ManageOrders, Checkout];

    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
