namespace OrchardCore.Commerce.Abstractions.Abstractions;

/// <summary>
/// Represents an object with an <see cref="Sku"/> property. This can identify a product.
/// </summary>
public interface ISkuHolder
{
    /// <summary>
    /// Gets or sets the product's SKU, which can also be used as an alias for the item.
    /// </summary>
    public string Sku { get; set; }
}
