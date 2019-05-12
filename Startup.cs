using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Handlers;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Migrations;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Money;
using OrchardCore.Commerce.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql.Indexes;

namespace OrchardCore.Commerce
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Product
            services.AddSingleton<IIndexProvider, ProductPartIndexProvider>();
            services.AddScoped<IDataMigration, ProductMigrations>();
            services.AddScoped<IContentAliasProvider, ProductPartContentAliasProvider>();
            services.AddScoped<IContentPartDisplayDriver, ProductPartDisplayDriver>();
            services.AddSingleton<ContentPart, ProductPart>();
            // Price
            services.AddScoped<IDataMigration, PriceMigrations>();
            services.AddScoped<IContentPartHandler, PricePartHandler>();
            services.AddScoped<IContentPartDisplayDriver, PricePartDisplayDriver>();
            services.AddSingleton<ContentPart, PricePart>();
            services.AddSingleton<IPriceProvider, PriceProvider>();
            services.AddSingleton<IPriceService, PriceService>();
            // Currency
            services.AddSingleton<IIndexProvider, CurrencyPartIndexProvider>();
            services.AddScoped<IDataMigration, CurrencyMigrations>();
            services.AddScoped<ICurrencyProvider, ContentItemCurrencyProvider>();
            //            services.AddSingleton<ICurrencyProvider, ContentItemCurrencyProvider>();
            //            services.AddSingleton<ICurrencyProvider, CurrencyProvider>();
            services.AddScoped<IContentPartDisplayDriver, CurrencyPartDisplayDriver>();
            services.AddScoped<IMoneyService, MoneyService>();
            //            services.AddSingleton<IMoneyService, MoneyService>();
            services.AddSingleton<ContentPart, CurrencyPart>();
            // Settings
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<ISite>, CommerceSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddTransient<IConfigureOptions<CommerceSettings>, CommerceSettingsConfiguration>();
        }
    }
}
