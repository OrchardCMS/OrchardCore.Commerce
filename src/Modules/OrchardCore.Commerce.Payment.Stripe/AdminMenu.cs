using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Payment.Stripe.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Commerce.Payment.Stripe;

public class AdminMenu : AdminMenuNavigationProviderBase
{
    public AdminMenu(IHttpContextAccessor hca, IStringLocalizer<AdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    { }

    protected override void Build(NavigationBuilder builder) =>
        builder.AddCommerce(T, commerce => commerce
            .Add(T["Stripe API"], T["Stripe API"], stripeApi => stripeApi
                .SiteSettings(StripeApiSettingsDisplayDriver.GroupId)
                .Permission(Permissions.ManageStripeApiSettings)
                .LocalNav()));
}
