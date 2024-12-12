using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Endpoints.Permissions;

public class ApiPermissions : AdminPermissionBase
{
    public static readonly Permission CommerceApi =
        new(nameof(CommerceApi), "Access Commerce APIs");
    public static readonly Permission CommerceShoppingCartApi =
        new(nameof(CommerceShoppingCartApi), "Access Commerce Shopping Cart APIs");
    public static readonly Permission CommerceOrderApi =
        new(nameof(CommerceOrderApi), "Access Commerce Order APIs");

    private static readonly IReadOnlyList<Permission> _adminPermissions =
    [
        CommerceApi,
        CommerceShoppingCartApi,
        CommerceOrderApi
    ];

    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
