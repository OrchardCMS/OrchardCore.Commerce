using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Payment.Exactly.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Commerce.Payment.Exactly;

public class AdminMenu : AdminMenuNavigationProviderBase
{
    public AdminMenu(IHttpContextAccessor hca, IStringLocalizer<AdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    { }

    protected override void Build(NavigationBuilder builder) =>
        builder.AddCommerce(T, commerce => commerce
            .Add(T["Exactly API"], T["Exactly API"], entry => entry
                .SiteSettings(ExactlySettingsDisplayDriver.EditorGroupId)
                .Permission(Permissions.ManageExactlySettings)
                .LocalNav()));
}
