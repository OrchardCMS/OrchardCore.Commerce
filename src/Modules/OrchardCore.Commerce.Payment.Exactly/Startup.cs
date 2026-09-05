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
using Refit;
using System;
using System.Net.Http;

namespace OrchardCore.Commerce.Payment.Exactly;

public class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration) => _shellConfiguration = shellConfiguration;

    public override void ConfigureServices(IServiceCollection services)
    {
        // Configuration, permission, admin things
        services.Configure<ExactlySettings>(_shellConfiguration.GetSection("OrchardCoreCommerce_Payment_Exactly"));
        services.AddTransient<IConfigureOptions<ExactlySettings>, ExactlySettingsConfiguration>();
        services.AddSiteDisplayDriver<ExactlySettingsDisplayDriver>();
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<INavigationProvider, AdminMenu>();

        foreach (var environment in new[] { PaymentEnvironment.Production, PaymentEnvironment.Sandbox })
        {
            var clientName = GetExactlyClientName(environment);

            services.AddHttpClient(clientName, (provider, client) =>
                {
                    var settings = provider
                        .GetRequiredService<IOptionsSnapshot<ExactlySettings>>()
                        .Value
                        .Get(environment);

                    client.BaseAddress = new Uri(settings.BaseAddress);
                })
                .AddHttpMessageHandler(provider =>
                    ActivatorUtilities.CreateInstance<ExactlyApiHandler>(provider, environment));

            services.AddKeyedScoped<IExactlyApi>(
                environment,
                (provider, _) => RestService.For<IExactlyApi>(
                    provider.GetRequiredService<IHttpClientFactory>().CreateClient(clientName)));

            services.AddKeyedScoped<IExactlyService>(
                environment,
                (provider, _) => ActivatorUtilities.CreateInstance<ExactlyService>(
                    provider,
                    environment,
                    provider.GetRequiredKeyedService<IExactlyApi>(environment)));

            services.AddScoped<IPaymentProvider>(provider =>
                ActivatorUtilities.CreateInstance<ExactlyPaymentProvider>(
                    provider,
                    environment,
                    provider.GetRequiredKeyedService<IExactlyService>(environment)));
        }
    }

    private static string GetExactlyClientName(PaymentEnvironment environment) =>
        $"OrchardCore.Commerce.Payment.Exactly.{environment}";
}
