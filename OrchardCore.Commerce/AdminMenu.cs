using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Settings;
using OrchardCore.Navigation;

namespace OrchardCore.Commerce;

public class AdminMenu : INavigationProvider
{
    private readonly IStringLocalizer<AdminMenu> _;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer) => _ = localizer;

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        builder
            .Add(_["Configuration"], configuration => configuration
                .Add(_["Settings"], settings => settings
                    .Add(_["Commerce"], _["Commerce"], entry => entry
                        .Action("Index", "Admin", new
                        {
                            area = "OrchardCore.Settings",
                            groupId = CommerceSettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManageCommerceSettings)
                        .LocalNav()
                    )));

        return Task.CompletedTask;
    }
}
