using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Tax.Constants;
using OrchardCore.Commerce.Tax.Drivers;
using OrchardCore.Commerce.Tax.Migrations;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.Tax.Navigation;
using OrchardCore.Commerce.Tax.Permissions;
using OrchardCore.Commerce.Tax.Services;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Commerce.Tax;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPermissionProvider, TaxRatePermissions>();
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddContentPart<TaxPart>()
            .WithMigration<TaxPartMigrations>();
        services.AddSiteDisplayDriver<TaxSettingsDisplayDriver>();
        services.AddScoped<INavigationProvider, TaxSettingsAdminMenu>();
    }
}

[Feature(FeatureIds.CustomTaxRates)]
public class CustomTaxRatesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ITaxRateSettingsHeaderProvider, TaxRateSettingsDisplayDriver>();
        services.AddSiteDisplayDriver<TaxRateSettingsDisplayDriver>();
        services.AddTransient<IConfigureOptions<TaxRateSettings>, TaxRateSettingsConfiguration>();
        services.AddScoped<INavigationProvider, TaxRateAdminMenu>();
    }
}
