using System;
using System.Collections.Generic;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Models
{
    /// <summary>
    /// Ashopping cart item
    /// </summary>
    [Serializable]
    public sealed class ShoppingCartItem : IEquatable<ShoppingCartItem>
    {
        /// <summary>
        /// The number of products
        /// </summary>
        public int Quantity { get; }

        /// <summary>
        /// The product
        /// </summary>
        public ContentItem Product { get; }

        /// <summary>
        /// The available prices
        /// </summary>
        public IList<IPrice> Prices { get; } = new List<IPrice>();

        /// <summary>
        /// Constructs a new shopping cart item
        /// </summary>
        /// <param name="quantity">The number of products</param>
        /// <param name="product">The product</param>
        public ShoppingCartItem(int quantity, ContentItem product)
        {
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            Quantity = quantity;
            Product = product ?? throw new ArgumentNullException(nameof(Product));
        }

        /// <summary>
        /// A string representation of the shopping cart item
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Quantity} x {Product.DisplayText}";
        }

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
            && (ReferenceEquals(this, obj) || Equals(obj as ShoppingCartItem));

        public bool Equals(ShoppingCartItem other)
            => other == null ? false : other.Quantity == Quantity && other.Product.Equals(Product);

        public override int GetHashCode() => HashCode.Combine(Product, Quantity);
    }
}
