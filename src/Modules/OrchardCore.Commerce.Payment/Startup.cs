using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Constants;
using OrchardCore.Commerce.Payment.Endpoints.Extensions;
using OrchardCore.Commerce.Payment.Services;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using System;

namespace OrchardCore.Commerce.Payment;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPermissionProvider, Permissions>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) =>
        routes.AddPaymentApiEndpoints();
}

[Feature(FeatureIds.DummyProvider)]
public class DummyProviderStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IPaymentProvider, DummyPaymentProvider>();
}
