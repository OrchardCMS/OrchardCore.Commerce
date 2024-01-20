using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Exactly.Models;
using OrchardCore.Commerce.Payment.Exactly.Services;
using OrchardCore.Modules;
using Refit;
using System;

namespace OrchardCore.Commerce.Payment.Exactly;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPaymentProvider, ExactlyPaymentProvider>();
        services.AddScoped<IExactlyService, ExactlyService>();

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
