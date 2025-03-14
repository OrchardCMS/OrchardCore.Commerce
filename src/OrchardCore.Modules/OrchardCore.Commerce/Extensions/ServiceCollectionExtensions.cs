using OrchardCore.Commerce.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="IProductListFilterProvider"/> type service <typeparamref name="TProvider"/> to the <paramref
    /// name="services"/>.
    /// </summary>
    public static IServiceCollection AddProductListFilterProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IProductListFilterProvider =>
        services.AddScoped<IProductListFilterProvider, TProvider>();
}
