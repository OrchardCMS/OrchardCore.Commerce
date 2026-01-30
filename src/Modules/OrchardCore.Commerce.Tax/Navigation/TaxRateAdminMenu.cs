using Lombiq.HelpfulLibraries.OrchardCore.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.Tax.Permissions;
using OrchardCore.Navigation;

namespace OrchardCore.Commerce.Tax.Navigation;

public class TaxRateAdminMenu : NavigationProviderBase
{
    protected override string NavigationName => "admin";

    public TaxRateAdminMenu(IHttpContextAccessor hca, IStringLocalizer<TaxRateAdminMenu> stringLocalizer)
        : base(hca, stringLocalizer)
    { }

    protected override void Build(NavigationBuilder builder) =>
        builder.AddCommerce(T, commerce => commerce
            .Add(T["Custom Tax Rates"], T["Custom Tax Rates"], entry => entry
                .SiteSettings(nameof(TaxRateSettings))
                .Permission(TaxRatePermissions.ManageCustomTaxRates)
                .LocalNav()));
}
