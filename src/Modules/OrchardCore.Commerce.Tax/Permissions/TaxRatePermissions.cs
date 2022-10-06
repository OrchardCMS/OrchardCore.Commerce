using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Permissions;

public class TaxRatePermissions : AdminPermissionBase
{
    public static readonly Permission ManageCustomTaxRates =
        new(nameof(ManageCustomTaxRates), "Manage Custom Tax Rates");

    protected override IEnumerable<Permission> AdminPermissions { get; } = new[]
    {
        ManageCustomTaxRates,
    };
}
