using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

public static class ProductInventoryProviderExtensions
{
    public static async Task<bool> IsAvailableAsync(
        this IEnumerable<IProductInventoryProvider> providers, string sku, IList<ShoppingCartItem> model)
    {
        var provider = await providers.GetFirstApplicableProviderAsync<IList<ShoppingCartItem>, IProductInventoryProvider>(model);

        return await provider.QueryInventoryAsync(sku) > 0;
    }
}
