using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Serialization;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// Ashopping cart item
    /// </summary>
    [JsonConverter(typeof(ShoppingCartItemConverter))]
    public sealed class ShoppingCartItem : IEquatable<ShoppingCartItem>
    {
        /// <summary>
        /// Constructs a new shopping cart item
        /// </summary>
        /// <param name="quantity">The number of products</param>
        /// <param name="product">The product</param>
        public ShoppingCartItem(
            int quantity,
            string productSku,
            IEnumerable<IProductAttributeValue> attributes = null,
            IEnumerable<PrioritizedPrice> prices = null)
        {
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            Quantity = quantity;
            ProductSku = productSku ?? throw new ArgumentNullException(nameof(productSku));
            Attributes = attributes is null
                ? new HashSet<IProductAttributeValue>()
                : new HashSet<IProductAttributeValue>(attributes);
            Prices = prices is null
                ? new List<PrioritizedPrice>().AsReadOnly()
                : new List<PrioritizedPrice>(prices).AsReadOnly();
        }

        /// <summary>
        /// The number of products
        /// </summary>
        public int Quantity { get; }

        /// <summary>
        /// The product SKU
        /// </summary>
        public string ProductSku { get; }

        /// <summary>
        /// The product attributes associated with this shopping cart line item
        /// </summary>
        public ISet<IProductAttributeValue> Attributes { get; }

        /// <summary>
        /// The available prices
        /// </summary>
        public IReadOnlyList<PrioritizedPrice> Prices { get; }

        /// <summary>
        /// Creates a new shopping cart item that is a clone of this, but with prices replaced with new ones.
        /// </summary>
        /// <param name="prices">The list of prices to add.</param>
        /// <returns>The new shopping cart item.</returns>
        public ShoppingCartItem WithPrices(IEnumerable<PrioritizedPrice> prices)
            => new ShoppingCartItem(Quantity, ProductSku, Attributes, prices);

        /// <summary>
        /// Creates a new shopping cart item that is a clone of this, but with an additional price.
        /// </summary>
        /// <param name="price">The price to add.</param>
        /// <returns>The new shopping cart item.</returns>
        public ShoppingCartItem WithPrice(PrioritizedPrice price)
            => new ShoppingCartItem(Quantity, ProductSku, Attributes, Prices.Concat(new[] { price }));

        /// <summary>
        /// Creates a new shopping cart item that is a clone of this, but with a different quantity.
        /// </summary>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public ShoppingCartItem WithQuantity(int quantity)
            => new ShoppingCartItem(quantity, ProductSku, Attributes, Prices);

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
            && (ReferenceEquals(this, obj) || Equals(obj as ShoppingCartItem));

        /// <summary>
        /// A string representation of the shopping cart item
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => Quantity + " x " + ProductSku
            + (Attributes.Count != 0 ? " (" + string.Join(", ", Attributes) + ")" : "");

        public bool Equals(ShoppingCartItem other)
            => other is null ? false : other.Quantity == Quantity && other.IsSameProductAs(this);

        public bool IsSameProductAs(ShoppingCartItem other)
            => ProductSku == other.ProductSku && Attributes.SetEquals(other.Attributes);

        public override int GetHashCode() => (ProductSku, Quantity, Attributes).GetHashCode();
    }
}
