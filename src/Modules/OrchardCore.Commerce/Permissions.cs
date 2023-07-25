using OrchardCore.Security.Permissions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce;

public class Permissions : IPermissionProvider
{
    public static readonly Permission ManageCurrencySettings = new(nameof(ManageCurrencySettings), "Manage Currency Settings");
    public static readonly Permission ManageStripeApiSettings = new(nameof(ManageStripeApiSettings), "Manage Stripe API Settings");
    public static readonly Permission ManagePriceDisplaySettings = new(nameof(ManagePriceDisplaySettings), "Manage Price Display Settings");
    public static readonly Permission ManageRegionSettings = new(nameof(ManageRegionSettings), "Manage Region Settings");
    public static readonly Permission ManageOrders = new(nameof(ManageOrders), "Manage Orders");
    public static readonly Permission Checkout = new(nameof(Checkout), "Ability to checkout");

    public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
        Task.FromResult<IEnumerable<Permission>>(new[]
        {
            ManageCurrencySettings,
            ManageStripeApiSettings,
            Checkout,
            ManageRegionSettings,
            ManageOrders,
            ManagePriceDisplaySettings,
        });

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
        new[]
        {
            new PermissionStereotype
            {
                Name = "Administrator",
                Permissions = new[]
                {
                    ManageCurrencySettings,
                    ManageStripeApiSettings,
                    Checkout,
                    ManageRegionSettings,
                    ManageOrders,
                    ManagePriceDisplaySettings,
                },
            },
        };
}
