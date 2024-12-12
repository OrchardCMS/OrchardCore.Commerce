using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;

public class ApiPermissions : AdminPermissionBase
{
    public static readonly Permission CommerceApiStripePayment =
        new(nameof(CommerceApiStripePayment), "Access Commerce Stripe Payment APIs");

    public static readonly Permission CommerceApiOrderStripe =
        new(nameof(CommerceApiStripePayment), "Access Commerce Stripe Order APIs");

    private static readonly IReadOnlyList<Permission> _adminPermissions = new[]
    {
        CommerceApiStripePayment,
    };
    protected override IEnumerable<Permission> AdminPermissions => _adminPermissions;
}
