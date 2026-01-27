using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Settings;
using OrchardCore.Navigation;

namespace OrchardCore.Commerce;

public class AdminMenu : AdminMenuNavigationProviderBase
{
    public AdminMenu(IHttpContextAccessor hca, IStringLocalizer<AdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    { }

    protected override void Build(NavigationBuilder builder) =>
        builder.AddCommerce(T, commerce => commerce
            .Add(T["Currency"], T["Currency"], entry => entry
                .SiteSettings(CurrencySettingsDisplayDriver.GroupId)
                .Permission(Permissions.ManageCurrencySettings)
                .LocalNav())
            .Add(T["Price Display"], T["Price Display"], entry => entry
                .SiteSettings(PriceDisplaySettingsDisplayDriver.GroupId)
                .Permission(Permissions.ManagePriceDisplaySettings)
                .LocalNav())
            .Add(T["Region"], T["Region"], region => region
                .SiteSettings(RegionSettingsDisplayDriver.GroupId)
                .Permission(Permissions.ManageRegionSettings)
                .LocalNav()));
}
