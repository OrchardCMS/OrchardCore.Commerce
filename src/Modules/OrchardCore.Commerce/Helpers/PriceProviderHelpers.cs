using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Helpers;
public static class PriceProviderHelpers
{
    public static async Task<IDictionary<string, ProductPart>> GetSkuProductsAsync(
        IList<ShoppingCartItem> items,
        IProductService productService) =>
        (await productService
            .GetProductsAsync(items.Select(item => item.ProductSku)
                .Distinct()
                .ToArray()))
            .ToDictionary(productPart => productPart.Sku);
}
