using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Settings;
using OrchardCore.Navigation;

namespace OrchardCore.Commerce;

public class AdminMenu : NavigationProviderBase
{
    protected override string NavigationName => "admin";

    public AdminMenu(IHttpContextAccessor hca, IStringLocalizer<AdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    { }

    protected override void Build(NavigationBuilder builder) =>
        builder
            .Add(T["Configuration"], configuration => configuration
                .Add(T["Commerce"], commerce => commerce
                    .Add(T["Currency"], T["Currency"], entry => entry
                        .Action("Index", "Admin", new // add AdminMenuConstants for these?
                        {
                            area = "OrchardCore.Settings",
                            groupId = CurrencySettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManageCurrencySettings)
                        .LocalNav())
                    .Add(T["Price Display"], T["Price Display"], entry => entry
                        .Action("Index", "Admin", new
                        {
                            area = "OrchardCore.Settings",
                            groupId = PriceDisplaySettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManagePriceDisplaySettings)
                        .LocalNav())
                    .Add(T["Stripe API"], T["Stripe API"], stripeApi => stripeApi
                        .Action("Index", "Admin", new
                        {
                            area = "OrchardCore.Settings",
                            groupId = StripeApiSettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManageStripeApiSettings)
                        .LocalNav())
                    .Add(T["Region"], T["Region"], region => region
                        .Action("Index", "Admin", new
                        {
                            area = "OrchardCore.Settings",
                            groupId = RegionSettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManageRegionSettings)
                        .LocalNav())));
}
