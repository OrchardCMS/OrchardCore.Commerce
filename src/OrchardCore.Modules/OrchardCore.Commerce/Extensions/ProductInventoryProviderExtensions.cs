using OrchardCore.Commerce.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

public static class ProductInventoryProviderExtensions
{
    public static async Task<bool> IsAvailableAsync(
        this IEnumerable<IProductInventoryProvider> providers, string sku, IList<ShoppingCartItem> model, string fullSku = null)
    {
        var provider = await providers.GetFirstApplicableProviderAsync(model) as IProductInventoryProvider;

        return await provider.QueryInventoryAsync(string.IsNullOrEmpty(fullSku) ? sku : fullSku) > 0;
    }
}
