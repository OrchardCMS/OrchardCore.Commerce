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
        builder
            .Add(T["Configuration"], configuration => configuration
                .Add(T["Commerce"], commerce => commerce
                    .Add(T["Custom Tax Rates"], entry => entry
                        .Action("Index", "Admin", new
                        {
                            area = "OrchardCore.Settings",
                            groupId = nameof(TaxRateSettings),
                        })
                        .Permission(TaxRatePermissions.ManageCustomTaxRates)
                        .LocalNav())));
}
