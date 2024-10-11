using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.Permissions;

public class ApiPermissions : AdminPermissionBase
{
    public static readonly Permission CommerceApi =
        new(nameof(CommerceApi), "Manage Commerce APIs");
    public static readonly Permission CommerceShoppingCartApi =
        new(nameof(CommerceShoppingCartApi), "Manage Commerce Shopping Cart APIs");

    private static readonly IReadOnlyList<Permission> _adminPermissions =
    [
        CommerceApi,
        CommerceShoppingCartApi
    ];

    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
