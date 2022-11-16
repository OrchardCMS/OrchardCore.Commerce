using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

public static class ProductInventoryServiceExtensions
{
    public static async Task<bool> IsAvailableAsync(this IProductInventoryService service, string sku) =>
        await service.QueryInventoryAsync(sku) > 0;
}
