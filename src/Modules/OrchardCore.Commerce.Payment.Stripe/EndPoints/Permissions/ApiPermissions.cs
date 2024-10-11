using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe.EndPoints.Permissions;

public class ApiPermissions : AdminPermissionBase
{
    public static readonly Permission CommerceApiStripePayment =
        new(nameof(CommerceApiStripePayment), "Manage Commerce Stripe Payment APIs");

    private static readonly IReadOnlyList<Permission> _adminPermissions = new[]
    {
        CommerceApiStripePayment,
    };
    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
