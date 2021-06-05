using System;
using InternationalAddress;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Money;
using Money.Abstractions;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Handlers;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Migrations;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Workflows.Helpers;
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
            services.AddScoped<IContentHandleProvider, ProductPartContentAliasProvider>();
            services.AddScoped<IProductService, ProductService>();
            services.AddContentPart<ProductPart>()
                .UseDisplayDriver<ProductPartDisplayDriver>();

            // Attributes
            services.AddContentField<BooleanProductAttributeField>()
                .UseDisplayDriver<BooleanProductAttributeFieldDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, BooleanProductAttributeFieldSettingsDriver>();

            services.AddContentField<NumericProductAttributeField>()
                .UseDisplayDriver<NumericProductAttributeFieldDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, NumericProductAttributeFieldSettingsDriver>();

            services.AddContentField<TextProductAttributeField>()
                .UseDisplayDriver<TextProductAttributeFieldDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextProductAttributeFieldSettingsDriver>();

            services.AddScoped<IProductAttributeProvider, ProductAttributeProvider>();
            services.AddScoped<IProductAttributeService, ProductAttributeService>();
            services.AddScoped<IPredefinedValuesProductAttributeService, PredefinedValuesProductAttributeService>();

            // Price
            services.AddScoped<IDataMigration, PriceMigrations>();

            services.AddContentPart<PricePart>()
                .UseDisplayDriver<PricePartDisplayDriver>()
                .AddHandler<PricePartHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, PricePartSettingsDisplayDriver>();

            services.AddScoped<IPriceProvider, PriceProvider>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<IPriceSelectionStrategy, SimplePriceStrategy>();

            // Price Variants
            services.AddScoped<IDataMigration, PriceVariantsMigrations>();

            services.AddContentPart<PriceVariantsPart>()
                .UseDisplayDriver<PriceVariantsPartDisplayDriver>()
                .AddHandler<PriceVariantsPartHandler>();

            services.AddScoped<IPriceProvider, PriceVariantProvider>();

            // Currency
            services.AddScoped<ICurrencyProvider, CurrencyProvider>();
            services.AddScoped<IMoneyService, MoneyService>();
            // No display currency selected. Fall back to default currency logic in MoneyService.
            services.AddScoped<ICurrencySelector, NullCurrencySelector>();

            // Shopping cart
            services.AddScoped<IShoppingCartHelpers, ShoppingCartHelpers>();
            services.AddActivity<ProductAddedToCartEvent, ProductAddedToCartEventDisplay>();

            // Orders
            services.AddScoped<IDataMigration, OrderMigrations>();
            services.AddScoped<IAddressFormatterProvider, AddressFormatterProvider>();

            services.AddContentPart<OrderPart>()
                .UseDisplayDriver<OrderPartDisplayDriver>();

            services.AddContentField<AddressField>()
                .UseDisplayDriver<AddressFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, AddressFieldSettingsDriver>();

            // Settings
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<ISite>, CommerceSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddTransient<IConfigureOptions<CommerceSettings>, CommerceSettingsConfiguration>();
        }
    }

    [Feature(CommerceConstants.Features.SessionCartStorage)]
    [RequireFeatures(CommerceConstants.Features.Core)]
    public class SessionCartStorageStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSession(options => { });
            // Shopping Cart
            services.AddScoped<IShoppingCartPersistence, SessionShoppingCartPersistence>();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            base.Configure(app, routes, serviceProvider);
            app.UseSession();
            routes.MapAreaControllerRoute(
                name: "ShoppingCart",
                areaName: "OrchardCore.Commerce",
                pattern: "shoppingcart/{action}",
                defaults: new { controller = "ShoppingCart", action = "Index" }
            );
        }
    }

    [Feature(CommerceConstants.Features.CommerceSettingsCurrencySelector)]
    [RequireFeatures(CommerceConstants.Features.Core)]
    public class CommerceSettingsCurrencySettingsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ICurrencySelector, CommerceSettingsCurrencySelector>();
        }
    }
}
