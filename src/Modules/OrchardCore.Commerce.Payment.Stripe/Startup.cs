using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Constants;
using OrchardCore.Commerce.Payment.Stripe.Drivers;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Extensions;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Permissions;
using OrchardCore.Commerce.Payment.Stripe.Handlers;
using OrchardCore.Commerce.Payment.Stripe.Indexes;
using OrchardCore.Commerce.Payment.Stripe.Migrations;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using Stripe;
using Stripe.Checkout;
using System;
using SubscriptionService = Stripe.SubscriptionService;

namespace OrchardCore.Commerce.Payment.Stripe;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IPermissionProvider, ApiPermissions>();

        services.AddScoped<IRequestOptionsService, RequestOptionsService>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        services.AddScoped<IPaymentIntentPersistence, PaymentIntentPersistence>();
        services.AddTransient<IConfigureOptions<StripeApiSettings>, StripeApiSettingsConfiguration>();

        services.AddContentPart<StripePaymentPart>().WithMigration<StripeMigrations>().WithIndex<OrderPaymentIndexProvider>();
        services.AddContentPart<StripeProductPart>();
        services.AddContentPart<StripePricePart>();
        services.AddContentPart<StripeProductFeaturePart>();
        services.AddContentPart<FeatureCollectionPart>();
        services.AddContentPart<PriceCollectionPart>();
        services.AddDataMigration<StripeProductMigrations>();

        services.AddScoped<IDisplayDriver<ISite>, StripeApiSettingsDisplayDriver>();

        services.AddScoped<IOrderContentTypeDefinitionExclusionProvider, StripeOrderContentTypeDefinitionExclusionProvider>();

        services.Configure<TemplateOptions>(option => option.MemberAccessStrategy.Register<StripePaymentProviderData>());

        services.AddContentSecurityPolicyProvider<StripeContentSecurityPolicyProvider>();

        services.AddDataMigration<StripeSessionMigrations>();

        services.AddScoped<IStripeHelperService, StripeHelperService>();
        services.AddScoped<IStripeSessionService, StripeSessionService>();
        services.AddScoped<IStripeCustomerService, StripeCustomerService>();
        services.AddScoped<IStripeSubscriptionService, StripeSubscriptionService>();
        services.AddScoped<IStripePaymentIntentService, StripePaymentIntentService>();
        services.AddScoped<IStripeConfirmationTokenService, StripeConfirmationTokenService>();

        services.AddScoped<SessionService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<SubscriptionService>();
        services.AddScoped<PaymentIntentService>();
        services.AddScoped<ConfirmationTokenService>();

        services.AddScoped<IStripeWebhookEventHandler, DefaultStripeWebhookEventHandler>();
        services.AddScoped<IStripeWebhookEventHandler, SubscriptionStripeWebhookEventHandler>();

        services.AddIndexProvider<StripeSessionDataIndexProvider>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
        routes.AddStripePaymentApiEndpoints();
}

[Feature(FeatureIds.StripeServices)]
public class StripeStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<SessionService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<SubscriptionService>();
        services.AddScoped<PaymentIntentService>();
        services.AddScoped<ConfirmationTokenService>();
    }
}

[Feature(FeatureIds.TestStripeServices)]
public class TestStripeStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.RemoveByImplementation<SessionService>();
        services.AddScoped<SessionService, TestSessionService>();

        services.RemoveByImplementation<CustomerService>();
        services.AddScoped<CustomerService, TestCustomerService>();
    }
}
