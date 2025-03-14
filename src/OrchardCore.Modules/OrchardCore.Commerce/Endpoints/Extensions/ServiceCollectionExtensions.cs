using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Commerce.Endpoints.Permissions;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Commerce.Endpoints.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommerceApiServices(this IServiceCollection services)
    {
        services.AddScoped<IPermissionProvider, ApiPermissions>();
        services.AddScoped<IShoppingCartService, ShoppingCartService>();
        return services;
    }
}
