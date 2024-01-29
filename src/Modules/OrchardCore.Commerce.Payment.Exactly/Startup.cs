using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Exactly.Drivers;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.Commerce.Payment.Exactly.Services;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using Refit;
using System;

namespace OrchardCore.Commerce.Payment.Exactly;

public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        // Payment services
        services.AddScoped<IPaymentProvider, ExactlyPaymentProvider>();
        //services.AddScoped<IExactlyService, ExactlyService>();

        // Configuration, permission, admin things
        services.Configure<ExactlySettings>(_shellConfiguration.GetSection("OrchardCoreCommerce_Payment_Exactly"));
        services.AddTransient<IConfigureOptions<ExactlySettings>, ExactlySettingsConfiguration>();
        services.AddScoped<IDisplayDriver<ISite>, ExactlySettingsDisplayDriver>();
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<INavigationProvider, AdminMenu>();

        // API client
        services.AddTransient<ExactlyApiHandler>();
        services.AddRefitClient<IExactlyApi>()
            .ConfigureHttpClient((provider, client) =>
            {
                var settings = provider
                    .GetRequiredService<IHttpContextAccessor>()
                    .HttpContext!
                    .RequestServices
                    .GetRequiredService<IOptionsSnapshot<ExactlySettings>>()
                    .Value;

                client.BaseAddress = new Uri(settings.BaseAddress);
            })
            .AddHttpMessageHandler<ExactlyApiHandler>();
    }
}
