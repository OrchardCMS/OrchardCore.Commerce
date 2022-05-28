using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OrchardCore.Commerce.Models;

/// <summary>
/// A shopping cart item.
/// </summary>
[JsonConverter(typeof(ShoppingCartItemConverter))]
public sealed class ShoppingCartItem : IEquatable<ShoppingCartItem>
{
    /// <summary>
    /// Gets the number of products.
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// Gets the product SKU.
    /// </summary>
    public string ProductSku { get; }

    /// <summary>
    /// Gets the product attributes associated with this shopping cart line item.
    /// </summary>
    public ISet<IProductAttributeValue> Attributes { get; }

    /// <summary>
    /// Gets the available prices.
    /// </summary>
    public IReadOnlyList<PrioritizedPrice> Prices { get; }

    public ShoppingCartItem(
        int quantity,
        string productSku,
        IEnumerable<IProductAttributeValue> attributes = null,
        IEnumerable<PrioritizedPrice> prices = null)
    {
        ArgumentNullException.ThrowIfNull(productSku);
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        Quantity = quantity;
        ProductSku = productSku;
        Attributes = attributes is null
            ? new HashSet<IProductAttributeValue>()
            : new HashSet<IProductAttributeValue>(attributes);
        Prices = prices is null
            ? new List<PrioritizedPrice>().AsReadOnly()
            : new List<PrioritizedPrice>(prices).AsReadOnly();
    }

    /// <summary>
    /// Creates a new shopping cart item that is a clone of this, but with prices replaced with new ones.
    /// </summary>
    /// <param name="prices">The list of prices to add.</param>
    /// <returns>The new shopping cart item.</returns>
    public ShoppingCartItem WithPrices(IEnumerable<PrioritizedPrice> prices) =>
        new(Quantity, ProductSku, Attributes, prices);

    /// <summary>
    /// Creates a new shopping cart item that is a clone of this, but with an additional price.
    /// </summary>
    /// <param name="price">The price to add.</param>
    /// <returns>The new shopping cart item.</returns>
    public ShoppingCartItem WithPrice(PrioritizedPrice price) =>
        new(Quantity, ProductSku, Attributes, Prices.Concat(new[] { price }));

    /// <summary>
    /// Creates a new shopping cart item that is a clone of this, but with a different quantity.
    /// </summary>
    public ShoppingCartItem WithQuantity(int quantity) =>
        new(quantity, ProductSku, Attributes, Prices);

    public override bool Equals(object obj) =>
        obj is not null && (ReferenceEquals(this, obj) || Equals(obj as ShoppingCartItem));

    public bool Equals(ShoppingCartItem other) =>
        other is not null && other.Quantity == Quantity && other.IsSameProductAs(this);

    /// <summary>
    /// A string representation of the shopping cart item.
    /// </summary>
    public override string ToString() =>
        $"{Quantity} x {ProductSku}" + (Attributes.Count != 0 ? $" ({string.Join(", ", Attributes)})" : string.Empty);

    public bool IsSameProductAs(ShoppingCartItem other) =>
        ProductSku == other.ProductSku && Attributes.SetEquals(other.Attributes);

    public override int GetHashCode() => (ProductSku, Quantity, Attributes).GetHashCode();
}
