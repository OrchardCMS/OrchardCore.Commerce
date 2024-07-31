using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.Permissions;

public class ApiPermissions : AdminPermissionBase
{
    public static readonly Permission CommerceApi = new("CommerceApi", "Manage Commerce APIs");

    private static readonly IReadOnlyList<Permission> _adminPermissions = new[]
    {
        CommerceApi,
    };
    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
