using Fluid;
using Lombiq.HelpfulLibraries.OrchardCore.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Abstractions.Fields;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.TagHelpers;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.AddressDataType;
using OrchardCore.Commerce.AddressDataType.Abstractions;
using OrchardCore.Commerce.Constants;
using OrchardCore.Commerce.ContentFields.Events;
using OrchardCore.Commerce.Controllers;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Endpoints.Extensions;
using OrchardCore.Commerce.Events;
using OrchardCore.Commerce.Fields;
using OrchardCore.Commerce.Handlers;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Liquid;
using OrchardCore.Commerce.Middlewares;
using OrchardCore.Commerce.Migrations;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.MoneyDataType;
using OrchardCore.Commerce.MoneyDataType.Abstractions;
using OrchardCore.Commerce.Payment.ViewModels;
using OrchardCore.Commerce.Promotion.Models;
using OrchardCore.Commerce.Services;
using OrchardCore.Commerce.Settings;
using OrchardCore.Commerce.Tax.Constants;
using OrchardCore.Commerce.Tax.Models;
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
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Helpers;
using System;
using YesSql.Indexes;
using static OrchardCore.Commerce.Tax.Constants.FeatureIds;

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
        services.AddScoped<IProductInventoryProvider, LocalInventoryProvider>();

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
        services.AddScoped<IProductAttributeProvider, TextProductAttributeProvider>();
        services.AddScoped<IProductAttributeProvider, BooleanProductAttributeProvider>();
        services.AddScoped<IProductAttributeProvider, NumericProductAttributeProvider>();
        services.AddScoped<IProductAttributeService, ProductAttributeService>();
        services.AddScoped<IPredefinedValuesProductAttributeService, PredefinedValuesProductAttributeService>();

        // Price
        services.AddScoped<IDataMigration, PriceMigrations>();
        services.AddSingleton<IIndexProvider, PriceIndexProvider>();

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

        // Tiered Prices
        services.AddScoped<IDataMigration, TieredPriceMigrations>();

        services.AddContentPart<TieredPricePart>()
            .UseDisplayDriver<TieredPricePartDisplayDriver>()
            .AddHandler<TieredPricePartHandler>();

        services.AddScoped<IPriceProvider, TieredPriceProvider>();

        // Currency
        services.AddScoped<ICurrencyProvider, CurrencyProvider>();
        services.AddScoped<IMoneyService, MoneyService>();

        // Shopping cart
        services.AddScoped<IShoppingCartHelpers, ShoppingCartHelpers>();
        services.AddScoped<IShoppingCartSerializer, ShoppingCartSerializer>();
        services.AddContentPart<ShoppingCartWidgetPart>()
            .UseDisplayDriver<ShoppingCartWidgetPartDisplayDriver>()
            .WithMigration<ShoppingCartWidgetMigrations>();
        services.AddScoped<IShoppingCartEvents, TaxShoppingCartEvents>();

        // Orders
        services.AddContentPart<OrderPart>()
            .UseDisplayDriver<OrderPartDisplayDriver>()
            .AddHandler<OrderPartHandler>();

        services.AddScoped<IAuthorizationHandler, OrderPermissionsAuthorizationHandler>();

        services.AddScoped<IDataMigration, OrderMigrations>();
        services.AddScoped<IAddressFormatterProvider, AddressFormatterProvider>();
        services.AddScoped<IOrderLineItemService, OrderLineItemService>();

        services.AddScoped<IContentTypeDefinitionDisplayDriver, OrderContentTypeDefinitionDisplayDriver>();

        // Region
        services.AddScoped<IRegionService, RegionService>();

        // Settings
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IDisplayDriver<ISite>, CurrencySettingsDisplayDriver>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddTransient<IConfigureOptions<CurrencySettings>, CurrencySettingsConfiguration>();
        services.AddScoped<IDisplayDriver<ISite>, PriceDisplaySettingsDisplayDriver>();
        services.AddScoped<IDisplayDriver<ISite>, RegionSettingsDisplayDriver>();
        services.AddTransient<IConfigureOptions<RegionSettings>, RegionSettingsConfiguration>();

        // Page
        services.AddScoped<IDataMigration, PageMigrations>();

        // Exposing models to liquid templates
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
                option.MemberAccessStrategy.Register<Abstractions.Models.Payment>();
                option.MemberAccessStrategy.Register<Amount, string>((obj, _) => obj.ToString());
                option.MemberAccessStrategy.Register<Amount, decimal>((obj, _) => obj.Value);
            })
            // Liquid filter to convert JsonNode value to Amount struct in liquid.
            .AddLiquidFilter<AmountConverterFilter>("amount")
            // Liquid filter to convert string, JToken or various models with "ProductSku" properties int an SKU and
            // then retrieve the corresponding content item.
            .AddLiquidFilter<ProductFilter>("product")
            // Liquid filter to create AddressFieldEditorViewModel.
            .AddLiquidFilter<AddressFieldEditorViewModelConverterFilter>("address_field_editor_view_model")
            // Liquid filter to create OrderLineItemViewModels and additional data.
            .AddLiquidFilter<OrderPartToOrderSummaryLiquidFilter>("order_part_to_order_summary")
            // Liquid filter to convert Amount, its JSON representation, or a number into Amount.ToString() including correct formatting and currency.
            .AddLiquidFilter<AmountToStringLiquidFilter>("amount_to_string");

        // Product List
        services.AddScoped<IProductListService, ProductListService>();
        services.AddProductListFilterProvider<ProductListTitleFilterProvider>();
        services.AddProductListFilterProvider<BasePriceFilterProvider>();
        services.AddScoped<IAppliedProductListFilterParametersProvider, QueryStringAppliedProductListFilterParametersProvider>();
        services.AddScoped<IDataMigration, ProductListMigrations>();
        services.AddContentPart<ProductListPart>()
            .UseDisplayDriver<ProductListPartDisplayDriver>();
        IProductAttributeDeserializer.AddSerializers(
            new TextProductAttributeDeserializer(),
            new BooleanProductAttributeDeserializer(),
            new NumericProductAttributeDeserializer());

        services.AddCommerceApiServices();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
           routes.AddShoppingCartApiEndpoints();
}

public sealed class FallbackPriceStartup : StartupBase
{
    public override int Order => int.MaxValue;

    public override void ConfigureServices(IServiceCollection services) =>
        // No display currency selected. Fall back to default currency logic in MoneyService.
        services.AddScoped<ICurrencySelector, NullCurrencySelector>();
}

public sealed class FallbackPriceStartup : StartupBase
{
    public override int Order => int.MaxValue;

    public override void ConfigureServices(IServiceCollection services) =>
        // No display currency selected. Fall back to default currency logic in MoneyService.
        services.AddScoped<ICurrencySelector, NullCurrencySelector>();
}

[RequireFeatures("OrchardCore.Workflows")]
public class WorkflowStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<ProductAddedToCartEvent, ProductAddedToCartEventDisplayDriver>();
        services.AddActivity<CartDisplayingEvent, CartDisplayingEventDisplayDriver>();
        services.AddActivity<CartVerifyingItemEvent, CartVerifyingItemEventDisplayDriver>();
        services.AddActivity<CartUpdatedEvent, CartUpdatedEventDisplayDriver>();
        services.AddActivity<CartLoadedEvent, CartLoadedEventDisplayDriver>();
        services.AddActivity<OrderCreatedEvent, OrderCreatedEventDisplayDriver>();

        services.AddScoped<IShoppingCartEvents, WorkflowShoppingCartEvents>();
        services.AddScoped<IOrderEvents, WorkflowOrderEvents>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<SiteSettingsPropertyDeploymentSource<RegionSettings>, SiteSettingsPropertyDeploymentStep<RegionSettings>>();
        services.AddScoped(ImplementationFactory);
    }

    private IDisplayDriver<DeploymentStep> ImplementationFactory(IServiceProvider serviceProvider)
    {
        var localizer = serviceProvider.GetService<IStringLocalizer<DeploymentStartup>>();

        return new SiteSettingsPropertyDeploymentStepDriver<RegionSettings>(
            localizer["Region settings"],
            localizer["Exports the region settings."]);
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
public class CurrencySettingsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<ICurrencySelector, CurrencySettingsSelector>();
}

[RequireFeatures(CommerceConstants.Features.Core, FeatureIds.Tax)]
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
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IOrderEvents, PromotionOrderEvents>();
        services.AddScoped<IShoppingCartEvents, PromotionShoppingCartEvents>();

        services
            .AddContentPart<DiscountPart>()
            .AddHandler<DiscountPartHandler>()
            .UseDisplayDriver<DiscountPartDisplayDriver>();

        services
            .AddContentPart<ProductPart>()
            .ForDisplayMode<DiscountPartDisplayDriver.StoredDiscountPartDisplayDriver>();

        services.AddScoped<IPromotionProvider, DiscountProvider>();
        services.AddScoped<IPromotionProvider, GlobalDiscountProvider>();
        services.AddScoped<IPromotionProvider, StoredDiscountProvider>();
    }
}

[RequireFeatures(CommerceConstants.Features.Core, CustomTaxRates)]
public class TaxRateStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddContentPart<TaxPart>()
            .UseDisplayDriver<TaxRateTaxPartDisplayDriver>();

        services.AddScoped<ITaxProvider, TaxRateTaxProvider>();
        services.AddScoped<TaxRateTaxProvider>();
    }
}

[RequireFeatures(CommerceConstants.Features.Core, "OrchardCore.Users.CustomUserSettings")]
public class UserSettingsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddContentPart<UserAddressesPart>()
            .WithMigration<UserAddressesMigrations>();

        services
            .AddContentPart<UserDetailsPart>()
            .WithMigration<UserDetailsMigrations>();

        services.AddScoped<IAddressFieldEvents, UserAddressFieldEvents>();
        services.AddScoped<IDisplayDriver<User>, UserAddressesUserDisplayDriver>();
        services.AddScoped<IOrderEvents, UserSettingsOrderEvents>();
        services.AddScoped<ICheckoutEvents, UserSettingsCheckoutEvents>();
    }

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

[RequireFeatures(Inventory.Constants.FeatureIds.Inventory)]
public class InventoryStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IProductEstimationContextUpdater, InventoryProductEstimationContextUpdater>();
        services.AddScoped<IOrderEvents, InventoryOrderEvents>();
        services.AddScoped<IProductInventoryService, ProductInventoryService>();
        services.AddScoped<ICheckoutEvents, InventoryCheckoutEvents>();
        services.AddScoped<IShoppingCartEvents, InventoryShoppingCartEvents>();
    }
}

[RequireFeatures("OrchardCore.ContentLocalization")]
public class ContentLocalizationStartup : StartupBase
{
    public override int Order => OrchardCoreCommerceConfigureOrder.AfterDefault;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDuplicateSkuResolver, LocalizationDuplicateSkuResolver>();

        services.RemoveImplementationsOf<IProductService>();
        services.AddScoped<IProductService, ContentLocalizationProductService>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
        app.UseMiddleware<LocalizationCurrencyRedirectMiddleware>();
}

[RequireFeatures(CommerceConstants.Features.Core, CommerceConstants.Features.Subscription)]
public class SubscriptionStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddContentPart<SubscriptionPart>()
            .WithMigration<SubscriptionMigrations>()
            .WithIndex<SubscriptionPartIndexProvider>();

        services.AddScoped<ISubscriptionService, SubscriptionService>();
    }
}
