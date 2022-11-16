using Fluid;
using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Events;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Handlers;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Liquid;
using OrchardCore.Commerce.Migrations;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.TagHelpers;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;
using OrchardCore.Workflows.Helpers;
using System;
using YesSql.Indexes;

namespace OrchardCore.Commerce;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        services.AddOrchardServices();
        services.AddScoped<IDataMigration, MvcTitleMigrations>();
        services.AddTagHelpers<MvcTitleTagHelper>();
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddScoped<IUserService, UserService>();

        // Product
        services.AddSingleton<IIndexProvider, ProductPartIndexProvider>();
        services.AddScoped<IDataMigration, ProductMigrations>();
        services.AddScoped<IContentHandleProvider, ProductPartContentAliasProvider>();
        services.AddScoped<IProductService, ProductService>();
        services.AddContentPart<ProductPart>()
            .UseDisplayDriver<ProductPartDisplayDriver>()
            .AddHandler<SkuValidationHandler>();

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
        services.AddScoped<IShoppingCartSerializer, ShoppingCartSerializer>();
        services.AddActivity<ProductAddedToCartEvent, ProductAddedToCartEventDisplay>();
        services.AddContentPart<ShoppingCartWidgetPart>()
            .UseDisplayDriver<ShoppingCartWidgetPartDisplayDriver>()
            .WithMigration<ShoppingCartWidgetMigrations>();
        services.AddScoped<IShoppingCartEvents, TaxShoppingCartEvents>();
        services.AddScoped<IShoppingCartEvents, PromotionShoppingCartEvents>();

        // Orders
        services.AddContentPart<OrderPart>()
            .UseDisplayDriver<OrderPartDisplayDriver>();

        services.AddContentField<AddressField>()
            .UseDisplayDriver<AddressFieldDisplayDriver>();
        services.AddScoped<IContentPartFieldDefinitionDisplayDriver, AddressFieldSettingsDriver>();

        services.AddScoped<IDataMigration, OrderMigrations>();
        services.AddScoped<IAddressFormatterProvider, AddressFormatterProvider>();

        // Region
        services.AddScoped<IRegionService, RegionService>();

        // Settings
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IDisplayDriver<ISite>, CurrencySettingsDisplayDriver>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddTransient<IConfigureOptions<CurrencySettings>, CurrencySettingsConfiguration>();
        services.AddScoped<IDisplayDriver<ISite>, StripeApiSettingsDisplayDriver>();
        services.AddTransient<IConfigureOptions<StripeApiSettings>, StripeApiSettingsConfiguration>();
        services.AddScoped<IDisplayDriver<ISite>, RegionSettingsDisplayDriver>();
        services.AddTransient<IConfigureOptions<RegionSettings>, RegionSettingsConfiguration>();

        // Page
        services.AddScoped<IDataMigration, PageMigrations>();

        // Promotion
        services.AddScoped<IPromotionService, PromotionService>();

        // Card Payment
        services.AddScoped<ICardPaymentService, CardPaymentService>();
        services.AddScoped<IDataMigration, StripeMigrations>();

        // Exposing models to liquid tempaltes
        services.Configure<TemplateOptions>(option =>
            {
                option.MemberAccessStrategy.Register<ShoppingCartViewModel>();
                option.MemberAccessStrategy.Register<ShoppingCartCellViewModel>();
                option.MemberAccessStrategy.Register<ShoppingCartLineViewModel>();
                option.MemberAccessStrategy.Register<CheckoutViewModel>();
                option.MemberAccessStrategy.Register<OrderPartViewModel>();
                option.MemberAccessStrategy.Register<OrderLineItemViewModel>();
                option.MemberAccessStrategy.Register<AddressFieldEditorViewModel>();
                option.MemberAccessStrategy.Register<OrderPart>();
                option.MemberAccessStrategy.Register<AddressField>();
                option.MemberAccessStrategy.Register<IPayment>();
                option.MemberAccessStrategy.Register<Amount, string>((obj, _) => obj.ToString());
                option.MemberAccessStrategy.Register<Amount, decimal>((obj, _) => obj.Value);
            })
            // Liquid filter to convert JToken value to Amount struct in liquid.
            .AddLiquidFilter<AmountConverterFilter>("amount")
            // Liquid filter to create AddressFiledEditorViewModel.
            .AddLiquidFilter<AddressFieldEditorViewModelConverterFilter>("address_field_editor_view_model");
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<RegionSettings>>();
        services.AddScoped<IDisplayDriver<DeploymentStep>>(serviceProvider =>
            {
                // It's the IStringLocalizer.
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
                var T = serviceProvider.GetService<IStringLocalizer<DeploymentStartup>>();
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
                return new SiteSettingsPropertyDeploymentStepDriver<RegionSettings>(
                    T["Region settings"],
                    T["Exports the region settings."]);
            });
        services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<RegionSettings>());
    }
}

[Feature(CommerceConstants.Features.SessionCartStorage)]
[RequireFeatures(CommerceConstants.Features.Core)]
public class SessionCartStorageStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSession(_ => { });

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
            defaults: new { controller = "ShoppingCart", action = "Index" });
    }
}

[Feature(CommerceConstants.Features.CurrencySettingsSelector)]
[RequireFeatures(CommerceConstants.Features.Core)]
public class CurrencySettingsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<ICurrencySelector, CurrencySettingsSelector>();
}

[RequireFeatures(CommerceConstants.Features.Core, Tax.Constants.FeatureIds.Tax)]
public class TaxStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddContentPart<PricePart>()
            .AddHandler<TaxPartAndPricePartHandler>();

        services.AddScoped<ITaxProvider, LocalTaxProvider>();
    }
}

[RequireFeatures(CommerceConstants.Features.Core, Promotion.Constants.FeatureIds.Promotion)]
public class PromotionStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddContentPart<DiscountPart>()
            .AddHandler<DiscountPartHandler>();

        services.AddScoped<IPromotionProvider, DiscountProvider>();
    }
}

[RequireFeatures(CommerceConstants.Features.Core, "OrchardCore.Users.CustomUserSettings")]
public class UserSettingsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services
            .AddContentPart<UserAddressesPart>()
            .WithMigration<UserAddressesMigrations>();

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        base.Configure(app, routes, serviceProvider);
        routes.MapAreaControllerRoute(
            name: nameof(UserController),
            areaName: "OrchardCore.Commerce",
            pattern: "user/{action}",
            defaults: new { controller = typeof(UserController).ControllerName(), action = "Index" });
    }
}
