using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Drivers;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Api;
using OrchardCore.Commerce.Payment.Stripe.Endpoints.Extensions;
using OrchardCore.Commerce.Payment.Stripe.Indexes;
using OrchardCore.Commerce.Payment.Stripe.Migrations;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using System;

namespace OrchardCore.Commerce.Payment.Stripe;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IPermissionProvider, Permissions>();

        services.AddScoped<IRequestOptionsService, RequestOptionsService>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        services.AddScoped<IPaymentIntentPersistence, PaymentIntentPersistence>();
        services.AddTransient<IConfigureOptions<StripeApiSettings>, StripeApiSettingsConfiguration>();

        services.AddContentPart<StripePaymentPart>().WithMigration<StripeMigrations>().WithIndex<OrderPaymentIndexProvider>();
        services.AddScoped<IDisplayDriver<ISite>, StripeApiSettingsDisplayDriver>();

        services.AddScoped<IOrderContentTypeDefinitionExclusionProvider, StripeOrderContentTypeDefinitionExclusionProvider>();

        services.Configure<TemplateOptions>(option => option.MemberAccessStrategy.Register<StripePaymentProviderData>());

        services.AddContentSecurityPolicyProvider<StripeContentSecurityPolicyProvider>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
           routes
               .AddStripeMiddlewareEndpoint()
               .AddStripePaymentApiEndpoints();
}
