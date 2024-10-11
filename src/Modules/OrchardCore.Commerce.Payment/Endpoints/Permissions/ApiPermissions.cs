using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Endpoints.Permissions;

public class ApiPermissions : AdminPermissionBase
{
    public static readonly Permission CommerceApiPayment =
        new(nameof(CommerceApiPayment), "Manage Commerce Payment APIs");

    private static readonly IReadOnlyList<Permission> _adminPermissions = new[]
    {
        CommerceApiPayment,
    };
    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
