using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Extensions;

public static class PriceProviderExtensions
{
    public static async Task<IDictionary<string, ProductPart>> GetSkuProductsAsync(
        this IProductService productService,
        IList<ShoppingCartItem> items) =>
            (await productService
                .GetProductsAsync(items.Select(item => item.ProductSku)))
                .Distinct()
                .ToDictionary(productPart => productPart.Sku);

    public static async Task<ShoppingCartItem> AddPriceAsync(this IPriceService priceService, ShoppingCartItem item) =>
        (await priceService.AddPricesAsync([item])).Single();
}
