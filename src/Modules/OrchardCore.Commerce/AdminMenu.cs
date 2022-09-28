using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Settings;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce;

public class AdminMenu : INavigationProvider
{
    private readonly IStringLocalizer<AdminMenu> T;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer) => T = localizer;

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase)) return Task.CompletedTask;

        builder
            .Add(T["Configuration"], configuration => configuration
                .Add(T["Settings"], settings => settings
                    .Add(T["Commerce"], T["Commerce"], entry => entry
                        .Action("Index", "Admin", new
                        {
                            area = "OrchardCore.Settings",
                            groupId = CommerceSettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManageCommerceSettings)
                        .LocalNav())
                    .Add(T["Stripe API"], T["Stripe API"], stripeApi => stripeApi
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = StripeApiSettingsDisplayDriver.GroupId })
                    .Permission(Permissions.ManageStripeApiSettings)
                    .LocalNav())
                    .Add(T["Region"], T["Region"], region => region
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = RegionSettingsDisplayDriver.GroupId })
                    .Permission(Permissions.ManageRegionSettings)
                    .LocalNav())));

        return Task.CompletedTask;
    }
}
