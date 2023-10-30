using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Drivers;
using OrchardCore.Commerce.Indexes;
using OrchardCore.Commerce.Migrations;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Stripe.Services;
using OrchardCore.Commerce.Services;
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
    }
}
