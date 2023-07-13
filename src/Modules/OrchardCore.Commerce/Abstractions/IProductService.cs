using OrchardCore.Commerce.Models;
using System.Collections.Generic;
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

    /// <summary>
    /// Returns the products that have the provided content item versions.
    /// </summary>
    Task<IEnumerable<ProductPart>> GetProductsByContentItemVersionsAsync(IEnumerable<string> contentItemVersions);

    /// <summary>
    /// Returns the key the product variant is identified by.
    /// </summary>
    string GetVariantKey(string sku);

    // The dictionary key should be the product full SKU, so the base SKU and ShoppingCartItem.GetVariantKeyFromAttributes(attributes)
    // separated by a dash, if it's not null or empty.
    /// <summary>
    /// Returns the full SKU of a Price Variant Product's variant.
    /// </summary>
    string GetOrderFullSku(ShoppingCartItem item, ProductPart productPart);

    /// <summary>
    /// Returns the exact variant of a product, as well as its identifying key, associated with the provided SKU.
    /// </summary>
    Task<(PriceVariantsPart Part, string VariantKey)> GetExactVariantAsync(string sku);
}
