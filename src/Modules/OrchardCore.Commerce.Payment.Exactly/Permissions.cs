using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Exactly;

public class Permissions : AdminPermissionBase
{
    public static readonly Permission ManageExactlySettings =
        new(nameof(ManageExactlySettings), "Manage Exactly settings.");

    protected override IEnumerable<Permission> AdminPermissions => [ManageExactlySettings];
}
