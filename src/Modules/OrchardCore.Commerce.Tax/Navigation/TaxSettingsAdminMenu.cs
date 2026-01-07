using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Tax.Drivers;
using OrchardCore.Commerce.Tax.Permissions;
using OrchardCore.Navigation;

namespace OrchardCore.Commerce.Tax.Navigation;

public class TaxSettingsAdminMenu : AdminMenuNavigationProviderBase
{
    public TaxSettingsAdminMenu(IHttpContextAccessor hca, IStringLocalizer<TaxSettingsAdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    {
    }

    protected override void Build(NavigationBuilder builder) =>
        builder
            .Add(T["Configuration"], configuration => configuration
                .Add(T["Commerce"], commerce => commerce
                    .Add(T["Tax"], T["Tax"], entry => entry
                        .Action("Index", "Admin", new
                        {
                            area = "OrchardCore.Settings",
                            groupId = TaxSettingsDisplayDriver.GroupId,
                        })
                        .Permission(TaxRatePermissions.ManageTaxSettings)
                        .LocalNav())));
}
