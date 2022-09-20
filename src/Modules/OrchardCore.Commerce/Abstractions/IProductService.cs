using OrchardCore.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions;

/// <summary>
/// Service for working with <see cref="ProductPart"/>.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Returns the products that have the provided SKUs.
    /// </summary>
    Task<IEnumerable<ProductPart>> GetProductsAsync(IEnumerable<string> skus);
}

public static class ProductServiceExtensions
{
    public static async Task<ProductPart> GetProductAsync(this IProductService service, string sku)
    {
        var dashIndex = sku.IndexOf(value: "-", StringComparison.InvariantCulture);
        return (await service.GetProductsAsync(new[] { dashIndex == -1 ? sku : sku[..dashIndex] })).FirstOrDefault();
    }

    public static async Task<IDictionary<string, ProductPart>> GetProductDictionaryAsync(
        this IProductService service,
        IEnumerable<string> skus) =>
        (await service.GetProductsAsync(skus)).ToDictionary(product => product.Sku);
}
