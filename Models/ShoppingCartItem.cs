using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        /// Constructs a new shopping cart item
        /// </summary>
        /// <param name="quantity">The number of products</param>
        /// <param name="product">The product</param>
        public ShoppingCartItem(int quantity, ContentItem product, ISet<IProductAttributeValue> attributes = null)
        {
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            Quantity = quantity;
            Product = product ?? throw new ArgumentNullException(nameof(Product));
            Attributes = attributes;
        }

        /// <summary>
        /// The number of products
        /// </summary>
        public int Quantity { get; }

        /// <summary>
        /// The product
        /// </summary>
        public ContentItem Product { get; }

        /// <summary>
        /// The product attributes associated with this shopping cart line item
        /// </summary>
        public ISet<IProductAttributeValue> Attributes { get; }

        /// <summary>
        /// The available prices
        /// </summary>
        public IList<IPrice> Prices { get; } = new List<IPrice>();

        public string Display(CultureInfo culture = null)
            => Quantity.ToString(culture ?? CultureInfo.InvariantCulture)
            + " x " + Product.DisplayText
            + (Attributes != null && Attributes.Count != 0 ? " (" + String.Join(", ", Attributes.Select(a => a.Display(culture))) + ")" : "");

        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj)
            && (ReferenceEquals(this, obj) || Equals(obj as ShoppingCartItem));

        /// <summary>
        /// A string representation of the shopping cart item
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => Quantity + " x " + Product.DisplayText
            + (Attributes != null && Attributes.Count != 0 ? " (" + String.Join(", ", Attributes) + ")" : "");

        public bool Equals(ShoppingCartItem other)
            => other is null ? false : other.Quantity == Quantity && other.Product.Equals(Product) && other.Attributes.SetEquals(Attributes);

        public override int GetHashCode() => HashCode.Combine(Product, Quantity, Attributes);
    }
}
