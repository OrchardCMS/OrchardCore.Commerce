using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Settings;
using OrchardCore.Navigation;
using static OrchardCore.Commerce.Constants.NavigationConstants;

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
                        .Action(ActionNames.Index, ControllerNames.Admin, new
                        {
                            area = $"{nameof(OrchardCore)}.{nameof(OrchardCore.Settings)}",
                            groupId = CurrencySettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManageCurrencySettings)
                        .LocalNav())
                    .Add(T["Price Display"], T["Price Display"], entry => entry
                        .Action(ActionNames.Index, ControllerNames.Admin, new
                        {
                            area = $"{nameof(OrchardCore)}.{nameof(OrchardCore.Settings)}",
                            groupId = PriceDisplaySettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManagePriceDisplaySettings)
                        .LocalNav())
                    .Add(T["Stripe API"], T["Stripe API"], stripeApi => stripeApi
                        .Action(ActionNames.Index, ControllerNames.Admin, new
                        {
                            area = $"{nameof(OrchardCore)}.{nameof(OrchardCore.Settings)}",
                            groupId = StripeApiSettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManageStripeApiSettings)
                        .LocalNav())
                    .Add(T["Region"], T["Region"], region => region
                        .Action(ActionNames.Index, ControllerNames.Admin, new
                        {
                            area = $"{nameof(OrchardCore)}.{nameof(OrchardCore.Settings)}",
                            groupId = RegionSettingsDisplayDriver.GroupId,
                        })
                        .Permission(Permissions.ManageRegionSettings)
                        .LocalNav())));
}
