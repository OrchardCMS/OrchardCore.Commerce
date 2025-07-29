using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Payment.Settings;
using OrchardCore.Navigation;

namespace OrchardCore.Commerce.Payment;

public class AdminMenu : AdminMenuNavigationProviderBase
{
    public AdminMenu(IHttpContextAccessor hca, IStringLocalizer<AdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    { }

    protected override void Build(NavigationBuilder builder) =>
        builder
            .Add(T["Configuration"], configuration => configuration
                .Add(T["Commerce"], commerce => commerce
                    .Add(T["Address Display"], T["Address Display"], entry => entry
                        .SiteSettings(CheckoutAddressSettingsDisplayDriver.GroupId)
                        .Permission(Permissions.Checkout)
                        .LocalNav())
                ));
}
