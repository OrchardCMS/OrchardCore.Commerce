using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.Tax.Permissions;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Tax.Navigation;

public class TaxRateAdminMenu : INavigationProvider
{
    private readonly IStringLocalizer<TaxRateAdminMenu> T;

    public TaxRateAdminMenu(IStringLocalizer<TaxRateAdminMenu> localizer) => T = localizer;

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (name?.EqualsOrdinalIgnoreCase("admin") != true) return Task.CompletedTask;

        builder
            .Add(T["Configuration"], configuration => configuration
                .Add(T["Settings"], settings => settings
                    .Add(T["Custom Tax Rates"], entry => entry
                        .Action("Index", "Admin", new
                        {
                            area = "OrchardCore.Settings",
                            groupId = nameof(TaxRateSettings),
                        })
                        .Permission(TaxRatePermissions.ManageCustomTaxRates)
                        .LocalNav())));

        return Task.CompletedTask;
    }
}
