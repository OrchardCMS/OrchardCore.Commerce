using Lombiq.HelpfulLibraries.OrchardCore.Users;
using OrchardCore.Security.Permissions;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Tax.Permissions;

public class TaxRatePermissions : AdminPermissionBase
{
    public static readonly Permission ManageCustomTaxRates =
        new(nameof(ManageCustomTaxRates), "Manage Custom Tax Rates");
    public static readonly Permission ManageTaxSettings =
        new(nameof(ManageTaxSettings), "Manage Tax Settings");

    protected override IEnumerable<Permission> AdminPermissions { get; } = [ManageCustomTaxRates, ManageTaxSettings];
}
