using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Drivers;
using OrchardCore.Commerce.Payment.Stripe.Indexes;
using OrchardCore.Commerce.Payment.Stripe.Migrations;
using OrchardCore.Commerce.Payment.Stripe.Models;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Commerce.Payment.Stripe;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddScoped<INavigationProvider, AdminMenu>();

        services.AddScoped<IRequestOptionsService, RequestOptionsService>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();
        services.AddScoped<IPaymentProvider, StripePaymentProvider>();
        services.AddScoped<IPaymentIntentPersistence, PaymentIntentPersistence>();
        services.AddTransient<IConfigureOptions<StripeApiSettings>, StripeApiSettingsConfiguration>();

        services.AddContentPart<StripePaymentPart>().WithMigration<StripeMigrations>().WithIndex<OrderPaymentIndexProvider>();
        services.AddScoped<IDisplayDriver<ISite>, StripeApiSettingsDisplayDriver>();

        services.AddScoped<IOrderContentTypeDefinitionExclusionProvider, StripeOrderContentTypeDefinitionExclusionProvider>();

        services.Configure<TemplateOptions>(option => option.MemberAccessStrategy.Register<StripePaymentProviderData>());
    }
}
