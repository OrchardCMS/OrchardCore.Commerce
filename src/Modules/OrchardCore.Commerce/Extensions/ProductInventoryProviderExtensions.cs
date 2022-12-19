using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

public static class ProductInventoryProviderExtensions
{
    public static async Task<bool> IsAvailableAsync(this IProductInventoryProvider provider, string sku) =>
        await provider.QueryInventoryAsync(sku) > 0;
}
