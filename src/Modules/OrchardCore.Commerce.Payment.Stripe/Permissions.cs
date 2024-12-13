using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe;

public class Permissions : AdminPermissionBase
{
    public static readonly Permission ManageStripeApiSettings = new(nameof(ManageStripeApiSettings), "Manage Stripe API Settings");

    private static readonly IReadOnlyList<Permission> _adminPermissions = [ManageStripeApiSettings];

    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
