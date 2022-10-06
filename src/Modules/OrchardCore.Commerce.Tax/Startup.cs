using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Tax.Drivers;
using OrchardCore.Commerce.Tax.Migrations;
using OrchardCore.Commerce.Tax.Models;
using OrchardCore.Commerce.Tax.Permissions;
using OrchardCore.Commerce.Tax.Services;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Tax;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddContentPart<TaxPart>()
            .WithMigration<TaxPartMigrations>();
}

[Feature(Constants.FeatureIds.CustomTaxRates)]
public class CustomTaxRatesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPermissionProvider, TaxRatePermissions>();

        services.AddScoped<IDisplayDriver<ISite>, TaxRateSettingsDisplayDriver>();
        services.AddTransient<IConfigureOptions<TaxRateSettings>, TaxRateSettingsConfiguration>();
    }
}
